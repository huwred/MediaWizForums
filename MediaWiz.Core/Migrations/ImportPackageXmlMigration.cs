using System.Xml.Linq;
using Microsoft.Extensions.Logging;
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
        private readonly IPackagingService _packagingService;
        public ImportPackageXmlMigration(
            IPackagingService packagingService,
            IMediaService mediaService,
            MediaFileManager mediaFileManager,
            MediaUrlGeneratorCollection mediaUrlGenerators,
            IShortStringHelper shortStringHelper,
            IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
            IMigrationContext context,ILogger<ImportPackageXmlMigration> logger
            )
            : base(packagingService,
                mediaService,
                mediaFileManager,
                mediaUrlGenerators,
                shortStringHelper,
                contentTypeBaseServiceProvider,
                context)
        {
            _packagingService = packagingService;

        }

        protected override void Migrate()
        {
            //var resourceName = "MediaWiz.Forums.Migrations.package.xml";
            //var packageXml = XDocument.Load(resourceName);
            //_packagingService.InstallCompiledPackageData(packageXml);
            ImportPackage.FromEmbeddedResource(GetType()).Do();
            Context.AddPostMigration<PublishRootBranchPostMigration>();
        }

    }
}
