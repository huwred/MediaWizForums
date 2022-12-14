using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Website.Models;

namespace MediaWiz.Forums.ViewModels
{
    public class ProfileViewModel
    {
        public ProfileModel ProfileModel { get; set; }

        public MemberIdentityUser CurrentUser { get; set; }
        public MemberIdentityUser MemberIdentity { get; set; }
        public string Username { get; set; }
        public IMember ViewMember { get; set; }
    }
}
