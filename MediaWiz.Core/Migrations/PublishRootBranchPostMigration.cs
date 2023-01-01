using System;
using System.Linq;
using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace MediaWiz.Forums.Migrations
{
    public class PublishRootBranchPostMigration : MigrationBase
    {
        private readonly ILogger<PublishRootBranchPostMigration> _logger;
        private readonly IContentService _contentService;
        private readonly IMemberTypeService _memberTypeService;
        private readonly IDataTypeService _dataTypeService;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IMemberGroupService _memberGroupService;
        private readonly IContentTypeService _contentTypeService;
        private readonly IExamineManager _examine;
        private readonly IMemberService _memberService;
        private readonly ILocalizationService _localizationService;

        public PublishRootBranchPostMigration(
            ILogger<PublishRootBranchPostMigration> logger,
            IContentService contentService,
            IMigrationContext context,
            IMemberGroupService memberGroupService,
            IMemberTypeService memberTypeService,
            IDataTypeService dataTypeService,
            IShortStringHelper shortStringHelper,
            IExamineManager examine,
            IContentTypeService contentTypeService,
            IMemberService memberService,ILocalizationService localizationService) : base(context)
        {
            _logger = logger;
            _memberGroupService = memberGroupService;
            _memberTypeService = memberTypeService;
            _dataTypeService = dataTypeService;
            _contentService = contentService;
            _shortStringHelper = shortStringHelper;
            _examine = examine;
            _contentTypeService = contentTypeService;
            _memberService = memberService;
            _localizationService = localizationService;
        }

        protected override void Migrate()
        {
            _logger.LogInformation("PublishRootBranchPostMigration");
            var contentForum = _contentService.GetRootContent().FirstOrDefault(x => x.ContentType.Alias == "forum");
            if (contentForum != null)
            {
                AddForumMemberType();
                AddMemberGroups();
                AddAnswerProperty();
                UpdatePostCounts();
                //AddDictionaryItems();
                //Make sure the Forum root has been published
                _contentService.SaveAndPublishBranch(contentForum, true);
            }
            else
            {
                _logger.LogWarning("The Forum is already installed");
            }
        }

        private void AddAnswerProperty()
        {
            try
            {
                var dataTypeDefinitions = _dataTypeService.GetAll().ToArray(); //.ToArray() because arrays are fast and easy.
                var truefalse = dataTypeDefinitions.FirstOrDefault(p => p.EditorAlias.ToLower() == "umbraco.truefalse" && p.Name.Contains("Resolved")); //we want the TrueFalse data type.
                
                var forumPost = _contentTypeService.Get("forumPost");
                if (forumPost != null && truefalse != null)
                {
                    if (!forumPost.PropertyTypes.Any(p => p.Alias == "answer"))
                    {
                        forumPost.AddPropertyType(new PropertyType(_shortStringHelper, truefalse)
                        {
                            Name = "Answer",
                            Alias = "answer",
                            Description = "Marked as solution/resolved."
                        });
                        _contentTypeService.Save(forumPost);
                    }

                }
            }
            catch (Exception e)
            {
                _logger.LogError(e,"Adding Ansered property");
                throw;
            }

        }
        private bool AddForumMemberType()
        {
            // do things on install
            bool saveMemberContent = false;

            string groupname = "Forum Settings";
            string mType = "forumMember";
            IMemberType memberContentType = _memberTypeService.Get(mType);
            if (memberContentType == null)
            {
                try
                {
                    _logger.LogDebug($"Creating MemberType={mType}");
                    memberContentType = new MemberType(_shortStringHelper, -1);
                    memberContentType.Name = "Forum Member";
                    memberContentType.Alias = "forumMember";
                    memberContentType.Icon = "icon-male-and-female";
                    _memberTypeService.Save(memberContentType);
                }
                catch (Exception e)
                {
                    _logger.LogError(e,"Error creating Membertype");
                }
            }


            var dataTypeDefinitions = _dataTypeService.GetAll().ToArray(); //.ToArray() because arrays are fast and easy.
            var truefalse = dataTypeDefinitions.FirstOrDefault(p => p.EditorAlias.ToLower() == "umbraco.truefalse"); //we want the TrueFalse data type.
            var textbox = dataTypeDefinitions.FirstOrDefault(p => p.EditorAlias.ToLower() == "umbraco.textbox"); //we want the TextBox data type.
            var datepicker = dataTypeDefinitions.FirstOrDefault(p => p.EditorAlias.ToLower() == "umbraco.datetime");

            try
            {
                
                
                _logger.LogDebug($"add a property group");
                if (!memberContentType.PropertyGroups.Contains(groupname))
                {
                    memberContentType.AddPropertyGroup(groupname,groupname); //add a property group, not needed, but I wanted it
                    
                    _memberTypeService.Save(memberContentType);
                }
                _logger.LogDebug($"add receiveNotifications");
                if(!memberContentType.PropertyTypeExists("receiveNotifications"))
                {
                    saveMemberContent = memberContentType.AddPropertyType(new PropertyType(_shortStringHelper,truefalse)
                    {
                        Name = "Receive Notifications",
                        Alias = "receiveNotifications",
                        Description = "Get an email when someone posts in a topic you are participating.",
                        Mandatory = false
                    }, groupname);
                    if (saveMemberContent)
                    {
                        memberContentType.SetMemberCanEditProperty("receiveNotifications",true);
                        memberContentType.SetMemberCanViewProperty("receiveNotifications",true);
                    }
                }
                else
                {
                    memberContentType.SetMemberCanEditProperty("receiveNotifications",true);
                    memberContentType.SetMemberCanViewProperty("receiveNotifications",true);
                }
                _logger.LogDebug($"add hasVerifiedAccount");
                if(!memberContentType.PropertyTypeExists("hasVerifiedAccount"))
                {
                    saveMemberContent = memberContentType.AddPropertyType(new PropertyType(_shortStringHelper,truefalse)
                    {
                        Name = "Has verified Email",
                        Alias = "hasVerifiedAccount",
                        Description = "User has verified their account.",
                        Mandatory = false
                    }, groupname);
                }
                _logger.LogDebug($"add resetGuid");
                if(!memberContentType.PropertyTypeExists("resetGuid"))
                {
                    saveMemberContent = memberContentType.AddPropertyType(new PropertyType(_shortStringHelper,textbox)
                    {
                        Name = "Reset Guid",
                        Alias = "resetGuid",
                        Description = "Guid set when user requests a password reset.",
                        Mandatory = false
                    }, groupname);
                    if(saveMemberContent)
                        memberContentType.SetIsSensitiveProperty("resetGuid",true);
                } 
                _logger.LogDebug($"add joinedDate");
                if(!memberContentType.PropertyTypeExists("joinedDate"))
                {
                    saveMemberContent = memberContentType.AddPropertyType(new PropertyType(_shortStringHelper,datepicker)
                    {
                        Name = "Joined date",
                        Alias = "joinedDate",
                        Description = "Date the user joined (validated email).",
                        Mandatory = false
                    }, groupname);
                    if(saveMemberContent)
                        memberContentType.SetMemberCanViewProperty("joinedDate",true);
                }

                if(!memberContentType.PropertyTypeExists("postCount"))
                {
                    _logger.LogDebug($"add postCount");

                    saveMemberContent = memberContentType.AddPropertyType(new PropertyType(_shortStringHelper,textbox)
                    {
                        Name = "Post Count",
                        Alias = "postCount",
                        Description = "Number of posts",
                        Mandatory = false
                    }, groupname);
                    if(saveMemberContent)
                        memberContentType.SetMemberCanViewProperty("postCount",true);
                }
                _logger.LogDebug($"add saveMemberContent");

                if (saveMemberContent)
                {
                    _memberTypeService.Save(memberContentType);//save the content type
                } 

            }
            catch (Exception e)
            {
                _logger.LogError( e, "Executing ForumInstallHandler:Add member types");
                return false;
            }

            return true;
        }
        private void AddMemberGroups()
        {
            if (_memberGroupService.GetByName("ForumMember") == null)
            {
                IMemberGroup membergroup = new MemberGroup();
                membergroup.Name = "ForumMember";
                _memberGroupService.Save(membergroup);
            }
            if (_memberGroupService.GetByName("ForumAdministrator") == null)
            {
                IMemberGroup admingroup = new MemberGroup();
                admingroup.Name = "ForumAdministrator";
                _memberGroupService.Save(admingroup);
            }
            if (_memberGroupService.GetByName("ForumModerator") == null)
            {
                IMemberGroup modgroup = new MemberGroup();
                modgroup.Name = "ForumModerator";
                _memberGroupService.Save(modgroup);
            }            

        }
        /// <summary>
        /// Updates the postcounts if members alreay exist
        /// </summary>
        /// <returns></returns>
        private long UpdatePostCounts()
        {
            long postcount = 0;
            try
            {
                if (_contentTypeService.GetAllContentTypeIds(new string[] {"forumPost"}).Any())
                {
                    if (_examine.TryGetIndex("ExternalIndex", out var index))
                    {
                        var searcher = index.Searcher;
                        foreach (var member in _memberService.GetMembersByPropertyValue("hasVerifiedAccount",true))
                        {
                            postcount = searcher.CreateQuery("content").NodeTypeAlias("forumpost").And()
                                .Field("postCreator", member.Name)
                                .Execute().TotalItemCount;
                            if (postcount > 0)
                            {
                                member.SetValue("postCount",postcount);
                                _memberService.Save(member);
                            }
                        }
                    }
                }

                return postcount;
            }
            catch (Exception e)
            {
                _logger.LogError( e, "Executing ForumInstallHandler:UpdatePostCounts");
                return -1;
            }

        }
        private void AddDictionaryItems()
        {
            try
            {
                var defLang = _localizationService.GetDefaultLanguageId();
                ILanguage lang = _localizationService.GetLanguageById(defLang.Value);
                if(!_localizationService.DictionaryItemExists("MediaWizForums"))
                {
                    var parentnode = new DictionaryItem("MediaWizForums");

                    var newitem = _localizationService.GetDictionaryItemByKey("Forums.ForgotPasswordView") ?? new DictionaryItem(parentnode.Key,"Forums.ForgotPasswordView");
                    _localizationService.AddOrUpdateDictionaryValue(newitem,lang, "/reset" );
                    _localizationService.Save(newitem);

                    newitem = _localizationService.GetDictionaryItemByKey("Forums.ForumUrl") ?? new DictionaryItem(parentnode.Key,"Forums.ForumUrl");
                    _localizationService.AddOrUpdateDictionaryValue(newitem,lang,"/" );
                    _localizationService.Save(newitem);

                    newitem = _localizationService.GetDictionaryItemByKey("Forums.LoginUrl") ?? new DictionaryItem(parentnode.Key,"Forums.LoginUrl");
                    _localizationService.AddOrUpdateDictionaryValue(newitem,lang,"/login" );
                    _localizationService.Save(newitem);
                    newitem = _localizationService.GetDictionaryItemByKey("Forums.CaptchaErrMsg") ?? new DictionaryItem(parentnode.Key,"Forums.LoginUrl");
                    _localizationService.AddOrUpdateDictionaryValue(newitem,lang,"Incorrect answer" );
                    _localizationService.Save(newitem);
                    newitem = _localizationService.GetDictionaryItemByKey("Forums.RegisterUrl") ?? new DictionaryItem(parentnode.Key,"Forums.RegisterUrl");
                    _localizationService.AddOrUpdateDictionaryValue(newitem,lang,"/register" );
                    _localizationService.Save(newitem);
                    newitem = _localizationService.GetDictionaryItemByKey("Forums.VerifyUrl") ?? new DictionaryItem(parentnode.Key,"Forums.VerifyUrl");
                    _localizationService.AddOrUpdateDictionaryValue(newitem,lang,"/verify" );
                    _localizationService.Save(newitem);
                    _localizationService.Save(lang);
                }


            }
            catch (Exception e)
            {
                _logger.LogError( e, "Executing AddDictionaryItems");

            }

        }

    }
}
