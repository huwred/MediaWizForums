using System.Linq;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace MediaWiz.Forums.Controllers
{
    public class JavascriptSurfaceController : SurfaceController
    {
        private readonly ILocalizationService _localizationService;

        public JavascriptSurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider,
            ILocalizationService localizationService) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _localizationService = localizationService;

        }

        /// <summary>
        /// Loads Umbraco Dictionary Items into a Javascript Object
        /// </summary>
        /// <param name="keys">The Dictionary items to return, default is All</param>
        /// <returns>JavascriptResult</returns>
        [HttpGet]
        public IActionResult LoadResources(string keys = null)
        {
            StringBuilder local = new StringBuilder("var local = {};");
            if(keys == null)
            {
                var rootItems = _localizationService.GetRootDictionaryItems();
                foreach (var item in rootItems)
                {
                    var dictionaryDescendants = _localizationService.GetDictionaryItemDescendants(item.Key);
                    var descendantDictionaryItems = dictionaryDescendants.ToList();
                    if(descendantDictionaryItems.Any())
                    {
                        foreach(var descendantItem in descendantDictionaryItems)
                        {
                            var translation = _localizationService.GetDictionaryItemByKey(descendantItem.ItemKey);
                            local.AppendLine($"local.{descendantItem.ItemKey.Replace(".","")} = \"{HttpUtility.HtmlEncode(translation)}\";");
                        }
                    }
                }
                return new  JavaScriptResult(local.ToString());
            }
            foreach (var item in keys.Split(","))
            {
                local.AppendLine($"local.{item.Replace(".","")} = \"{_localizationService.GetDictionaryItemByKey(item)}\";");
            }

            return new  JavaScriptResult(local.ToString());
        }

    }

    public class JavaScriptResult : ContentResult
    {
        public JavaScriptResult(string script)
        {
            this.Content = script;
            this.ContentType = "application/javascript";
        }
    }
}
