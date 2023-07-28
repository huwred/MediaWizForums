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
            //register the views as templates before importing the package
            var asm = Assembly.GetExecutingAssembly();
            var xmlpackage = "package.xml";
            var templatepackage = "packagetemplates.xml";

            //var mediawizmaster = _fileService.GetTemplate("mediawizMaster") ?? _fileService.CreateTemplateWithIdentity("MediawizMaster", "mediawizMaster",null);
            //var master = _fileService.GetTemplate("forumMaster") ?? _fileService.CreateTemplateWithIdentity("ForumMaster", "forumMaster",null);
            
            //var forumTemplates = new[] { "forum", "forumPost", "login", "members", "profile", "register", "reset", "verify", "forumSearch", "activeTopics" };
            if (ForumDoctypes != null)
            {
                xmlpackage = "forumpackage.xml";
                templatepackage = "forumtemplates.xml";
                //forumTemplates = new[] { "forum", "forumPost", "forumLogin", "forumMembers", "forumProfile", "forumRegister", "forumReset", "forumVerify", "forumSearch", "activeTopics" };
            }
            else
            {

            }
            //foreach (var template in forumTemplates)
            //{
            //    var found = _fileService.GetTemplate(template);
            //    if (found == null)
            //    {
            //        _fileService.CreateTemplateWithIdentity(template.FirstCharToUpper(), template,null,master);
            //    }
            //}
            //Now the templates are registered we can import the package xml, but first lets remove the empty templates
            using(var stream = asm.GetManifestResourceStream("MediaWiz.Forums.Migrations." + templatepackage))
            {
                var templateXml = XDocument.Load(stream);
                _packagingService.InstallCompiledPackageData(templateXml);
            }

            using(var stream = asm.GetManifestResourceStream("MediaWiz.Forums.Migrations." + xmlpackage))
            {
                var packageXml = XDocument.Load(stream);
                _packagingService.InstallCompiledPackageData(packageXml);
            }

            Context.AddPostMigration<PublishRootBranchPostMigration>();
        }
    }
}
