using System;
using System.Linq;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Packaging;

namespace MediaWiz.Forums.Migrations;

public class PublishDocTypeChangesMigration : PackageMigrationBase
{

    public PublishDocTypeChangesMigration(IPackagingService packagingService, IMediaService mediaService, MediaFileManager mediaFileManager, MediaUrlGeneratorCollection mediaUrlGenerators, IShortStringHelper shortStringHelper, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider, IMigrationContext context, IOptions<PackageMigrationSettings> packageMigrationsSettings) : base(packagingService, mediaService, mediaFileManager, mediaUrlGenerators, shortStringHelper, contentTypeBaseServiceProvider, context, packageMigrationsSettings)
    {


    }

    protected override void Migrate()
    {
        Context.AddPostMigration<PublishDocTypeChanges>();
    }
}

public class PublishDocTypeChanges : MigrationBase
{
    public readonly IContentTypeService _contentTypeService;
    public readonly IShortStringHelper _shortStringHelper;

    public PublishDocTypeChanges(IMigrationContext context,IContentTypeService contentTypeService, IShortStringHelper shorty) : base(context)
    {
        _contentTypeService = contentTypeService;
        _shortStringHelper = shorty;
    }

    protected override void Migrate()
    {
        var container = _contentTypeService.GetContainers("Test", 1)?.FirstOrDefault();

        if (container == null)
        {
            //-1 = the parent id
            var attempt = _contentTypeService.CreateContainer(-1, Guid.NewGuid(), "Test");
            container = attempt.Result.Entity;
            _contentTypeService.SaveContainer(container);
        }

        IContentType newCT = _contentTypeService.Get("searchPage");
        if (newCT == null)
        {
            newCT = new ContentType(_shortStringHelper, container.Id) { Alias = "searchPage", Icon = "icon-clothes-hanger color-deep-purple" };
            newCT.Name = "Search Page";
            _contentTypeService.Save(newCT);
        }
        else
        {
            newCT.Alias = "searchPageTest";
            newCT.Name = "Search Page Test";
                
            _contentTypeService.Save(newCT);
        }
    }
}