using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.Lucene.Search;
using Examine.Search;
using MediaWiz.Forums.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace MediaWiz.Forums.Controllers
{

    public class ForumSearchController : RenderController
    {
        private readonly IPublishedContentQuery _publishedContentQuery;
        private readonly IExamineManager _examineManager;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly ServiceContext _serviceContext;
        public ForumSearchController(ILogger<SearchPageController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor, IVariationContextAccessor variationContextAccessor, ServiceContext context,IPublishedContentQuery publishedContentQuery,IExamineManager examineManager)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _variationContextAccessor = variationContextAccessor;
            _serviceContext = context;
            _publishedContentQuery = publishedContentQuery;
            _examineManager = examineManager;
        }
        public override IActionResult Index()
        {

            // you are in control here!
            // create our ViewModel based on the PublishedContent of the current request:
            // set our custom properties
            SearchViewModel searchPageViewModel = new SearchViewModel(CurrentPage,
                new PublishedValueFallback(_serviceContext, _variationContextAccessor))
            {
                //do the search
                query = "",
                searchIn = "",
                TotalResults = 0,
                PagedResult = null
            };

            
            // return our custom ViewModel
            return CurrentTemplate(searchPageViewModel);

        }

        [HttpGet]
        public IActionResult Index([FromQuery(Name = "page")] int page, [FromQuery(Name = "searchIn")] string searchIn, [FromQuery(Name = "query")] string query)
        {
            if (String.IsNullOrWhiteSpace(query))
            {
                SearchViewModel pageViewModel = new SearchViewModel(CurrentPage,
                    new PublishedValueFallback(_serviceContext, _variationContextAccessor))
                {
                    //do the search
                    query = "",
                    searchIn = "",
                    TotalResults = 0,
                    PagedResult = null
                };

            
                // return our custom ViewModel
                return CurrentTemplate(pageViewModel);
            }
            ISearchResults results = null;

            var textFields = new List<string>();

            switch (searchIn)
            {
                case "Subject":
                    textFields.Add("nodeName");
                    break;
                case "Message":
                    textFields.Add("message");
                    break;
                case "Username":
                    textFields.Add("author");
                    break;
                default:
                    textFields.Add("message");
                    searchIn = "Message";
                    break;
            }

            if (page == 0) page = 1;

            int pageIndex = page - 1;
            int pageSize = CurrentPage.Value<int>("intPageSize");

            if (_examineManager.TryGetIndex("ForumIndex", out var index))
            {
                var options = new LuceneSearchOptions()
                {
                    AllowLeadingWildcard = true
                };
                var searcher = index.Searcher;
                
                //var value = "" + query + "*";
                var search = (LuceneSearchQueryBase)searcher.CreateQuery(IndexTypes.Content);
                    search.QueryParser.AllowLeadingWildcard = true;
                    search.Field("__NodeTypeAlias","forumPost").And()
                //.Field("postType","1").And()
                    .GroupedOr(textFields.ToArray(), query.Boost(2.0f))
                    .Or()
                    .GroupedOr(textFields.ToArray(), new ExamineValue(Examineness.ComplexWildcard, "*" + query));
                //var query = (LuceneSearchQueryBase)searcher.CreateQuery("content");
                
                results = searcher.Search(search.ToString());
            }
            var totalResults = results.TotalItemCount;
            var pagedResults = results.Skip(pageIndex * pageSize).Take(pageSize);
            var pagedResultsAsContent = _publishedContentQuery.Content(pagedResults.Select(x => x.Id));

            SearchViewModel searchPageViewModel = new SearchViewModel(CurrentPage, new PublishedValueFallback(_serviceContext, _variationContextAccessor))
            {
                //do the search
                query = query,
                searchIn = searchIn,
                TotalResults = totalResults,
                PagedResult = pagedResultsAsContent
            };
            //then return the custom model:
            return CurrentTemplate(searchPageViewModel);
        }

    }
}