using System.Reflection;
using MediaWiz.Forums.Extensions;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using MediaWiz.Forums.Helpers;
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
        private readonly IPackagingService _packagingService;
        private readonly IOptions<ForumConfigOptions> _forumOptions;
        private string ForumDoctypes => _forumOptions.Value.ForumDoctypes;

        public ImportPackageXmlMigration(IPackagingService packagingService, IMediaService mediaService, MediaFileManager mediaFileManager, MediaUrlGeneratorCollection mediaUrlGenerators, IShortStringHelper shortStringHelper, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider, IMigrationContext context, IOptions<PackageMigrationSettings> packageMigrationsSettings
            ,IFileService fileService,IOptions<ForumConfigOptions> forumOptions) : base(packagingService, mediaService, mediaFileManager, mediaUrlGenerators, shortStringHelper, contentTypeBaseServiceProvider, context, packageMigrationsSettings)
        {
            _fileService = fileService;
            _packagingService = packagingService;
            _forumOptions = forumOptions;
            
        }

        protected override void Migrate()
        {

            //set the default values for the xml files to import
            var xmlpackage = "package.xml";
            var templatepackage = "packagetemplates.xml";
            if (ForumDoctypes != null) //If the override value is set load the alternate xml files
            {
                xmlpackage = "forumpackage.xml";
                templatepackage = "forumtemplates.xml";
            }
            var asm = Assembly.GetExecutingAssembly();
            //Import the templates
            using(var stream = asm.GetManifestResourceStream("MediaWiz.Forums.Migrations." + templatepackage))
            {
                var templateXml = XDocument.Load(stream);
                _packagingService.InstallCompiledPackageData(templateXml);
            }
            //Import doctypes and content nodes
            using(var stream = asm.GetManifestResourceStream("MediaWiz.Forums.Migrations." + xmlpackage))
            {
                var packageXml = XDocument.Load(stream);
                _packagingService.InstallCompiledPackageData(packageXml);
            }

            Context.AddPostMigration<PublishRootBranchPostMigration>();
        }
    }
}
