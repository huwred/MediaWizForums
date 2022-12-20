using System;
using System.Collections.Generic;
using Examine;
using MediaWiz.Forums.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Examine;

namespace MediaWiz.Forums.Indexing
{
    public class ForumIndexValueSetBuilder : IValueSetBuilder<IContent>
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IUmbracoContextFactory _context;
        public ForumIndexValueSetBuilder(IServiceScopeFactory scopeFactory,IUmbracoContextFactory context)
        {
            _scopeFactory = scopeFactory;
            _context = context;

        }
        public IEnumerable<ValueSet> GetValueSets(params IContent[] contents)
        {
            _context.EnsureUmbracoContext();
            using var scope = _scopeFactory.CreateScope();
            
            var _cacheService = scope.ServiceProvider.GetRequiredService<IForumCacheService>();
            var _publishedContent = scope.ServiceProvider.GetRequiredService<IPublishedContentQuery>();

            foreach (var content in contents)
            {
                var post = _publishedContent.Content(content.Id);
                var cacheInfo = _cacheService.GetPost(post, "Topic_" + content.Id,new TimeSpan(0,0,10));

                var indexValues = new Dictionary<string, object>
                {
                    //["name"] = content.Name,
                    ["id"] = content.Id,
                    ["message"] = content.GetValue<string>("forumDescription") ?? content.GetValue<string>("postBody"),
                    ["author"] = content.GetValue<string>("postCreator"),
                    ["title"] = content.GetValue<string>("forumTitle") ?? content.GetValue<string>("postTitle"),
                    ["edited"] = content.GetValue<DateTime?>("editDate"),
                    ["posttype"] = content.GetValue<int>("postType"),
                    ["updated"] = content.UpdateDate,
                    ["lastpost"] = cacheInfo.latestPost == DateTime.MinValue ? content.CreateDate : cacheInfo.latestPost
                };

                yield return new ValueSet(content.Id.ToString(), "content", indexValues);
            }
        }
    }
}
