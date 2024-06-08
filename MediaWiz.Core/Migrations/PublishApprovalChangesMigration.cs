using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;

namespace MediaWiz.Forums.Migrations
{
    public class PublishApprovalChangesMigration : MigrationBase
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly IContentTypeService _contentTypeService;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly ILogger<PublishApprovalChangesMigration> _logger;
        public PublishApprovalChangesMigration(IMigrationContext context,IDataTypeService dataTypeService, 
            IContentTypeService contentTypeService,IShortStringHelper shortStringHelper,
            ILogger<PublishApprovalChangesMigration> logger) : base(context)
        {
            _dataTypeService = dataTypeService;
            _contentTypeService = contentTypeService;
            _shortStringHelper = shortStringHelper;
            _logger = logger;
        }

        protected override void Migrate()
        {
            AddRequireApprovalProperty();
            AddApprovalProperty();

        }
        private void AddApprovalProperty()
        {
            try
            {
                var dataTypeDefinitions = _dataTypeService.GetAll().ToArray(); //.ToArray() because arrays are fast and easy.
                var truefalse = dataTypeDefinitions.FirstOrDefault(p => p.EditorAlias.ToLower() == "umbraco.truefalse" && p.Name.Contains("Resolved")); //we want the TrueFalse data type.
                
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
                    if (!forumPost.PropertyTypes.Any(p => p.Alias == "requireapproval"))
                    {
                        var approvalPropertyType = new PropertyType(_shortStringHelper, truefalse)
                        {
                            Key = Guid.Parse("FBF4EB1E-ECD3-438C-B36B-472B59E983B8"),
                            Name = "Require Approval",
                            Alias = "requireapproval",
                            Description = "Post requires approval.",

                        };
                        
                        forumPost.AddPropertyType(approvalPropertyType,"general");
                        _contentTypeService.Save(forumPost);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e,"Adding Approval property");
                throw;
            }

        }
        private void AddRequireApprovalProperty()
        {
            try
            {
                var dataTypeDefinitions = _dataTypeService.GetAll().ToArray(); //.ToArray() because arrays are fast and easy.
                var truefalse = dataTypeDefinitions.FirstOrDefault(p => p.EditorAlias.ToLower() == "umbraco.truefalse" && p.Name.Contains("Resolved")); //we want the TrueFalse data type.
                
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
                _logger.LogError(e,"Adding Approval property");
                throw;
            }

        }

    }
}