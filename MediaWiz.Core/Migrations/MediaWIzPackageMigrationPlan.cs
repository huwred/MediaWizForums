using System;
using System.Linq;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Packaging;

namespace MediaWiz.Forums.Migrations
{
    public class MediaWizPackageMigrationPlan : PackageMigrationPlan
    {
        public MediaWizPackageMigrationPlan()
            : base("MediaWiz Forums")
        {
        }

        protected override void DefinePlan()
        {

            To<ImportPackageXmlMigration>(new Guid("87B98517-9A06-457D-9445-59F56CFD1A32"))
                .To<UpdateForumAnswer>(new Guid("E55F155E-4BF1-4E25-8E96-24913E05251A"));
        }
    }

    public class UpdateForumAnswer : MigrationBase
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IDataTypeService _dataTypeService;
        public UpdateForumAnswer(IMigrationContext context,
            IContentTypeService contentTypeService, IShortStringHelper shortStringHelper,IDataTypeService dataTypeService) : base(context)
        {
            _contentTypeService = contentTypeService;
            _shortStringHelper = shortStringHelper;
            _dataTypeService = dataTypeService;
        }

        protected override void Migrate()
        {
            var dataTypeDefinitions = _dataTypeService.GetAll().ToArray(); //.ToArray() because arrays are fast and easy.
            var truefalse = dataTypeDefinitions.FirstOrDefault(p => p.EditorAlias.ToLower() == "umbraco.truefalse"); //we want the TrueFalse data type.
            
            var forumPost = _contentTypeService.Get("forumPost");
            forumPost.AddPropertyType(new PropertyType(_shortStringHelper, truefalse)
            {
                Name = "Answer",
                Alias = "answer",
                Description = "Marked as solution/resolved."
            }, "General");
            _contentTypeService.Save(forumPost);
        }
    }
}
