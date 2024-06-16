using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using MediaWiz.Forums.Helpers;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Core.Services.Implement;

namespace MediaWiz.Forums.Migrations
{
    public class PublishApprovalChangesMigration : MigrationBase
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly IContentTypeService _contentTypeService;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly ILogger<PublishApprovalChangesMigration> _logger;
        private readonly IOptions<ForumConfigOptions> _forumOptions;
        private readonly IPackagingService _packagingService;
        private string ForumDoctypes => _forumOptions.Value.ForumDoctypes;

        public PublishApprovalChangesMigration(IMigrationContext context,IDataTypeService dataTypeService, 
            IContentTypeService contentTypeService,IShortStringHelper shortStringHelper,
            ILogger<PublishApprovalChangesMigration> logger,IOptions<ForumConfigOptions> forumOptions,
            IPackagingService packagingService) : base(context)
        {
            _dataTypeService = dataTypeService;
            _contentTypeService = contentTypeService;
            _shortStringHelper = shortStringHelper;
            _logger = logger;
            _packagingService = packagingService;
            _forumOptions = forumOptions;
        }

        protected override void Migrate()
        {
            //load updated templates from package xml files
            var xmlpackage = "package-approval.xml";
            var templatepackage = "packagetemplates.xml";
            if (ForumDoctypes != null) //If the override value is set load the alternate xml files
            {
                templatepackage = "forumtemplates.xml";
            }
            var asm = Assembly.GetExecutingAssembly();
            //Import doctypes and content nodes
            using(var stream = asm.GetManifestResourceStream("MediaWiz.Forums.Migrations." + xmlpackage))
            {
                var packageXml = XDocument.Load(stream);
                _packagingService.InstallCompiledPackageData(packageXml);
            }            
            //Import the templates
            using(var stream = asm.GetManifestResourceStream("MediaWiz.Forums.Migrations." + templatepackage))
            {
                var templateXml = XDocument.Load(stream);
                _packagingService.InstallCompiledPackageData(templateXml);
            }

            //Add new properties to the doc types
            AddForumRequireApprovalProperty();
            AddPostApprovalProperty();

        }
        private void AddPostApprovalProperty()
        {
            try
            {
                var dataTypeDefinitions = _dataTypeService.GetAll().ToArray(); //.ToArray() because arrays are fast and easy.
                var truefalse = dataTypeDefinitions.FirstOrDefault(p => p.EditorAlias.ToLower() == "umbraco.truefalse" && p.Name.Contains("Approved")); //we want the TrueFalse data type.
                var numeric = dataTypeDefinitions.FirstOrDefault(p => p.EditorAlias.ToLower() == "umbraco.integer" && p.Name.Contains("Numeric")); //we want the TrueFalse data type.
                
                var forumPost = _contentTypeService.Get("forumPost");
                if (forumPost != null && truefalse != null)
                {
                    if (!forumPost.PropertyTypes.Any(p => p.Alias == "approved"))
                    {
                        var approvedPropertyType = new PropertyType(_shortStringHelper, truefalse)
                        {
                            Key = Guid.Parse("6BF64B84-B947-438C-BA89-EB66E9CFFFB8"),
                            Name = "Approved",
                            Alias = "approved",
                            Description = "Post has been approved.",

                        };
                        
                        forumPost.AddPropertyType(approvedPropertyType,"general");
                        _contentTypeService.Save(forumPost);
                    }
                    if (!forumPost.PropertyTypes.Any(p => p.Alias == "unapprovedReplies"))
                    {
                        var approvalPropertyType = new PropertyType(_shortStringHelper, truefalse)
                        {
                            Key = Guid.Parse("FBF4EB1E-ECD3-438C-B36B-472B59E983B8"),
                            Name = "Unapproved Replies",
                            Alias = "unapprovedReplies",
                            Description = "COntains unapproved replies.",

                        };
                        
                        forumPost.AddPropertyType(approvalPropertyType,"general");
                        _contentTypeService.Save(forumPost);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e,"Adding Post Approval properties");
                throw;
            }

        }
        private void AddForumRequireApprovalProperty()
        {
            try
            {
                var dataTypeDefinitions = _dataTypeService.GetAll().ToArray(); //.ToArray() because arrays are fast and easy.
                var truefalse = dataTypeDefinitions.FirstOrDefault(p => p.EditorAlias.ToLower() == "umbraco.truefalse" && p.Name.Contains("Require")); //we want the TrueFalse data type.
                
                var forum = _contentTypeService.Get("forum");
                if (forum != null && truefalse != null)
                {
                    if (!forum.PropertyTypes.Any(p => p.Alias == "requireApproval"))
                    {
                        var approvedPropertyType = new PropertyType(_shortStringHelper, truefalse)
                        {
                            Key = Guid.Parse("84C6263A-E95D-4C0D-B425-30AE795A4A6C"),
                            Name = "Require Approval",
                            Alias = "requireApproval",
                            Description = "Posts require approval."
                        };
                        
                        forum.AddPropertyType(approvedPropertyType,"general");
                        _contentTypeService.Save(forum);
                    }

                }
            }
            catch (Exception e)
            {
                _logger.LogError(e,"Adding Forum Approval property");
                throw;
            }

        }

    }
}