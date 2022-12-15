using System.Collections.Generic;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace MediaWiz.Forums.ViewModels
{
    public class SearchViewModel : PublishedContentWrapped
    {

        public string query { get; set; }
        public string searchIn { get; set; }
        public long TotalResults { get; set; }
        public IEnumerable<IPublishedContent> PagedResult { get; set; }

        public SearchViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback) : base(content, publishedValueFallback)
        {
        }
    }
}