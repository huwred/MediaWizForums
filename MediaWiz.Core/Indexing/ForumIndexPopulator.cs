using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;

namespace MediaWiz.Forums.Indexing
{
    public class ForumIndexPopulator : IndexPopulator
    {
        private readonly IContentService _contentService;
        private readonly ForumIndexValueSetBuilder _forumIndexValueSetBuilder;

        public ForumIndexPopulator(IContentService contentService, ForumIndexValueSetBuilder forumIndexValueSetBuilder)
        {
            _contentService = contentService;
            _forumIndexValueSetBuilder = forumIndexValueSetBuilder;
            RegisterIndex("ForumIndex");
        }
        protected override void PopulateIndexes(IReadOnlyList<IIndex> indexes)
        {
            foreach (var index in indexes)
            {
                var roots = _contentService.GetRootContent();

                index.IndexItems(_forumIndexValueSetBuilder.GetValueSets(roots.ToArray()));

                foreach (var root in roots)
                {
                    var valueSets = _forumIndexValueSetBuilder.GetValueSets(_contentService.GetPagedDescendants(root.Id, 0, Int32.MaxValue, out _).ToArray());
                    index.IndexItems(valueSets);
                }
            }

        }
    }
}
