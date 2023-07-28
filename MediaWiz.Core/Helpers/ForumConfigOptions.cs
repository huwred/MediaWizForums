
namespace MediaWiz.Forums.Helpers
{
    public class ForumConfigOptions
    {
        public const string MediaWizOptions = "MediaWizOptions";

        public int MaxFileSize { get; set; }
        public string[] AllowedFiles { get; set; }

        public bool UniqueFilenames { get; set; }

        public string ForumDoctypes { get; set; }
    }
    
}

