using Examine;
using Examine.Lucene;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Util;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;

namespace MediaWiz.Forums.Indexing
{
    public class ConfigureForumIndexOptions : IConfigureNamedOptions<LuceneDirectoryIndexOptions>
    {
        private readonly IOptions<IndexCreatorSettings> _settings;
        private readonly IPublicAccessService _publicAccessService;
        private readonly IScopeProvider _scopeProvider;

        public ConfigureForumIndexOptions(
            IOptions<IndexCreatorSettings> settings,
            IPublicAccessService publicAccessService,
            IScopeProvider scopeProvider)
        {
            _settings = settings;
            _publicAccessService = publicAccessService;
            _scopeProvider = scopeProvider;
        }

        public void Configure(string name, LuceneDirectoryIndexOptions options)
        {
            if (name.Equals("ForumIndex"))
            {
                options.Analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);

                options.FieldDefinitions = new(
                    new("id", FieldDefinitionTypes.Integer),
                    //new("name", FieldDefinitionTypes.FullText),
                    new("title", FieldDefinitionTypes.FullText),
                    new("message", FieldDefinitionTypes.FullText),
                    new("author", FieldDefinitionTypes.FullText),
                    new("edited", FieldDefinitionTypes.DateTime),
                    new("postType", FieldDefinitionTypes.Long),
                    new("updated", FieldDefinitionTypes.DateTime),
                    new ("lastpost", FieldDefinitionTypes.DateTime)
                    );

                options.UnlockIndex = true;

                options.Validator = new ContentValueSetValidator(true, false, _publicAccessService, _scopeProvider, includeItemTypes: new[] { "forumPost" });
                
                if (_settings.Value.LuceneDirectoryFactory == LuceneDirectoryFactory.SyncedTempFileSystemDirectoryFactory)
                {
                    // if this directory factory is enabled then a snapshot deletion policy is required
                    options.IndexDeletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());
                }
            }
        }
        
        public void Configure(LuceneDirectoryIndexOptions options)
        {
            throw new System.NotImplementedException();
        }
    }
}
