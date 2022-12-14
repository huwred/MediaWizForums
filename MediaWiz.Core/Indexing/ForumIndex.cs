using Examine.Lucene;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;

namespace MediaWiz.Forums.Indexing
{
    public class ForumIndex : UmbracoExamineIndex
    {
        public ForumIndex(
            ILoggerFactory loggerFactory, 
            string name, 
            IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions, 
            IHostingEnvironment hostingEnvironment, 
            IRuntimeState runtimeState)
            : base(loggerFactory, 
                name, 
                indexOptions, 
                hostingEnvironment, 
                runtimeState)
        {
        }
    }
}
