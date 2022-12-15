using Examine;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Examine;

namespace MediaWiz.Forums.Indexing
{
    public class ExamineComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddExamineLuceneIndex<ForumIndex, ConfigurationEnabledDirectoryFactory>("ForumIndex");

            builder.Services.AddSingleton<ForumIndexValueSetBuilder>();

            builder.Services.AddSingleton<IIndexPopulator, ForumIndexPopulator>();
        }
    }
}
