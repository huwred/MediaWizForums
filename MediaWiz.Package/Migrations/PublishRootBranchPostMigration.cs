using System;
using System.Linq;
using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace MediaWiz.Package.Migrations
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
            IMemberService memberService) : base(context)
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
        }

        protected override void Migrate()
        {
            var contentForum = _contentService.GetRootContent().FirstOrDefault(x => x.ContentType.Alias == "forum");
            if (contentForum != null)
            {
                AddMemberGroups();
                CreateForumMemberType();
                UpdatePostCounts();
                _contentService.SaveAndPublishBranch(contentForum, true);

            }
            else
            {
                _logger.LogWarning("The installed Home page was not found");
            }
        }

        private bool CreateForumMemberType()
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
                    saveMemberContent = memberContentType.AddPropertyType(new PropertyType(_shortStringHelper,textbox)
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
                membergroup.Name = "ForumMemberr";
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
    }
}
