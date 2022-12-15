using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MediaWiz.Forums.Models
{
    public class ForumsPostModel
    {
        public int Id { get; set; }
        public int ParentId { get; set; }

        [DisplayName("Title")] public string Title { get; set; }

        [Required]
        [DisplayName("Reply")]
        public string Body { get; set; }

        [Required] public int AuthorId { get; set; }

        public bool IsTopic { get; set; }

        public string returnPath { get; set; }
    }

    public class ForumsForumModel
    {
        public int Id { get; set; }
        public int ParentId { get; set; }

        [Required]
        [DisplayName("Title")] 
        public string Title { get; set; }

        [Required]
        [DisplayName("Introduction")]
        public string Introduction { get; set; }

        [DisplayName("Allow Posts")]
        public bool AllowPosts { get; set; }
        [DisplayName("Allow Images")]
        public bool AllowImages { get; set; }
    }
}

