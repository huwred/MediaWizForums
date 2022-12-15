using MediaWiz.Forums.Extensions;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Packaging;

namespace MediaWiz.Forums.Migrations
{
    public class ImportPackageXmlMigration : PackageMigrationBase
    {
        private readonly IFileService _fileService;
        public ImportPackageXmlMigration(
            IPackagingService packagingService,
            IMediaService mediaService,
            MediaFileManager mediaFileManager,
            MediaUrlGeneratorCollection mediaUrlGenerators,
            IShortStringHelper shortStringHelper,
            IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
            IMigrationContext context,
            IOptions<PackageMigrationSettings> packageMigrationsSettings,IFileService fileService) 
            : base(
                packagingService,
                mediaService,
                mediaFileManager,
                mediaUrlGenerators,
                shortStringHelper,
                contentTypeBaseServiceProvider,
                context,
                packageMigrationsSettings)
        {
            _fileService = fileService;
        }

        protected override void Migrate()
        {
            //register the views as templates first
            _fileService.CreateTemplateWithIdentity("ForumMaster", "forumMaster",null);
            var master = _fileService.GetTemplate("forumMaster");
            
            if (master != null)
            {
                var templatesToFind = new[] { "forum", "forumPost", "login", "members", "profile", "register", "reset", "verify", "searchPage", "activeTopics" };
                foreach (var template in templatesToFind)
                {
                    _fileService.CreateTemplateWithIdentity(template.FirstCharToUpper(), template,null,master);
                }

            }
            //Now the templates are registered we can import the package xml, but first lets remove the empty templates
            ImportPackage.FromEmbeddedResource<ImportPackageXmlMigration>().Do();
            Context.AddPostMigration<PublishRootBranchPostMigration>();
        }

    }
}
