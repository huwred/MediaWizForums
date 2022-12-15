using System;

namespace MediaWiz.Forums.Models
{
    public class ForumCacheItem : IComparable
    {
        public string lastPostUrl { get; set; }
        public int Id { get; set; }
        public int Count { get; set; }
        public int TopicCount { get; set; }
        public int ReplyCount { get; set; }
        public int Page { get; set; }
        public int Views { get; set; }
        public string lastpostAuthor { get; set; }
        public int lastpostAuthorId { get; set; }
        public DateTime latestPost { get; set; }
        public DateTime latestEdit { get; set; }
        public int CompareTo(object obj)
        {
            return latestPost.CompareTo(((ForumCacheItem)obj).latestPost);
        }
    }

    public class TopicCacheItem : IComparable{
        public int Views { get; set; }
        public int ReplyCount { get; set; }
        public string lastpostAuthor { get; set; }
        public int lastpostAuthorId { get; set; }
        public DateTime latestPost { get; set; }
        public DateTime latestEdit { get; set; }
        public string lastPostUrl { get; set; }

        public int CompareTo(object obj)
        {
            return latestPost.CompareTo(((TopicCacheItem)obj).latestPost);
        }
    }
}