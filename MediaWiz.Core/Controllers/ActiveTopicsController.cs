using System;
using System.Linq;
using Examine;
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


namespace MediaWiz.Forums.Controllers
{
    public class ActiveTopicsController : RenderController
    {
        private readonly IPublishedContentQuery _publishedContentQuery;
        private readonly IExamineManager _examineManager;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly ServiceContext _serviceContext;

        public ActiveTopicsController(ILogger<ActiveTopicsController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor, IVariationContextAccessor variationContextAccessor, ServiceContext context, IPublishedContentQuery publishedContentQuery, IExamineManager examineManager)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _variationContextAccessor = variationContextAccessor;
            _serviceContext = context;
            _publishedContentQuery = publishedContentQuery;
            _examineManager = examineManager;

        }

        /// <summary>
        /// Added date range query
        /// </summary>
        [HttpGet]
        public IActionResult Index([FromQuery(Name = "page")] int page, [FromQuery(Name = "TopicsSince")] string query, [FromQuery(Name = "showonly")] string filter)
        {
            ISearchResults results = null;
            var today = DateTime.Now;
            long min = today.Ticks;
            long max = today.Ticks;
            query ??= "7d";

            switch (query)
            {
                case "30m" :
                    min = today.AddMinutes(-30).Ticks;
                    break;
                case "60m" :
                    min = today.AddHours(-1).Ticks;
                    break;
                case "1d" :
                    min = today.AddDays(-1).Ticks;
                    break;
                case "7d" :
                    min = today.AddDays(-7).Ticks;
                    break;
                case "1m" :
                    min = today.AddMonths(-1).Ticks;
                    break;
                case "1y" :
                    min = today.AddYears(-1).Ticks;
                    break;
            }

            int pageIndex = page - 1;
            if(pageIndex < 0) {pageIndex = 0;}
            int pageSize = CurrentPage.Value<int>("intPageSize");
            if(filter != null)
            {
                if (_examineManager.TryGetIndex("ForumIndex", out var index))
                {
                    var searcher = index.Searcher;

                    var examineQuery = searcher.CreateQuery(IndexTypes.Content)
                    .Field("postType", "Topic");

                    if (filter == "noreply")
                    {
                        examineQuery = examineQuery.And().Field("replies", "0");
                    }
                    if (filter == "unsolved")
                    {
                        examineQuery = examineQuery.And().Field("answered", "0");
                    }
                    results = examineQuery.OrderByDescending(new SortableField[] { new SortableField("updateDate") }).Execute();
                }
            }
            else
            {
                if (_examineManager.TryGetIndex("ForumIndex", out var index))
                {
                    var searcher = index.Searcher;

                    var examineQuery = searcher.CreateQuery(IndexTypes.Content)
                    .Field("postType", "Topic")
                        //.And().Field("approved", "1")
                        .And().RangeQuery<long>(new string[] { "updated" }, min, max)
                        .OrderByDescending(new SortableField[] { new SortableField("updateDate") });
                
                    results = examineQuery.Execute();
                }
            }

            if (results != null)
            {
                var pagedResults = results.Skip(pageIndex * pageSize).Take(pageSize);
                var totalResults = results.TotalItemCount;
                var ids = pagedResults.Select(x => x.Id);
                var pagedResultsAsContent = _publishedContentQuery.Content(ids);

                SearchViewModel searchPageViewModel = new SearchViewModel(CurrentPage, new PublishedValueFallback(_serviceContext, _variationContextAccessor))
                {
                    //do the search
                    query = query?.ToString(),
                    searchIn = "",
                    TotalResults = totalResults,
                    PagedResult = pagedResultsAsContent
                };
                //then return the custom model:
                return CurrentTemplate(searchPageViewModel);
            }
            return CurrentTemplate(new SearchViewModel(CurrentPage, new PublishedValueFallback(_serviceContext, _variationContextAccessor)));
        }

        public IActionResult Sort([FromQuery(Name = "page")] int page, [FromQuery(Name = "query")] string query)
        {
            ISearchResults results = null;
            if (string.IsNullOrWhiteSpace(query))
            {
                query = "updated";
            }

            int pageIndex = page - 1;
            int pageSize = CurrentPage.Value<int>("intPageSize");

            if (_examineManager.TryGetIndex("ForumIndex", out var index))
            {
                var searcher = index.Searcher;
                var test = searcher.Search("* AND -postType");
                var examineQuery = searcher.CreateQuery(IndexTypes.Content)
                    .Field("postType", "Topic")
                    .OrderByDescending(new SortableField[] { new SortableField("updateDate") });
                    //.Execute(/*maxResults: pageSize*(pageIndex + 1)*/);

                results = examineQuery.Execute();
            }

            if (results != null)
            {
                var pagedResults = results.Skip(pageIndex * pageSize).Take(pageSize);
                var totalResults = results.TotalItemCount;
                var pagedResultsAsContent = _publishedContentQuery.Content(pagedResults.Select(x => x.Id));

                SearchViewModel searchPageViewModel = new SearchViewModel(CurrentPage, new PublishedValueFallback(_serviceContext, _variationContextAccessor))
                {
                    //do the search
                    query = query,
                    searchIn = "",
                    TotalResults = totalResults,
                    PagedResult = pagedResultsAsContent
                };
                //then return the custom model:
                return CurrentTemplate(searchPageViewModel);
            }
            return CurrentTemplate(new SearchViewModel(CurrentPage, new PublishedValueFallback(_serviceContext, _variationContextAccessor)));
        }

    }
}