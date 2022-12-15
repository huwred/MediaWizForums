using System;
using System.Collections.Generic;
using Examine;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Examine;

namespace MediaWiz.Forums.Indexing
{
    public class ForumIndexValueSetBuilder : IValueSetBuilder<IContent>
    {
        public IEnumerable<ValueSet> GetValueSets(params IContent[] contents)
        {
            foreach (var content in contents)
            {
                var indexValues = new Dictionary<string, object>
                {
                    //["name"] = content.Name,
                    ["id"] = content.Id,
                    ["message"] = content.GetValue<string>("forumDescription") ?? content.GetValue<string>("postBody"),
                    ["author"] = content.GetValue<string>("postCreator"),
                    ["title"] = content.GetValue<string>("forumTitle") ?? content.GetValue<string>("postTitle"),
                    ["edited"] = content.GetValue<DateTime?>("editDate"),
                    ["posttype"] = content.GetValue<int>("postType"),
                    ["updated"] = content.UpdateDate
                };

                yield return new ValueSet(content.Id.ToString(), "content", indexValues);
            }
        }
    }
}
