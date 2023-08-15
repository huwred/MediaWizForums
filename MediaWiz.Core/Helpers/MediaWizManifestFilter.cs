using System.Collections.Generic;
using Umbraco.Cms.Core.Manifest;

namespace MediaWiz.Forums.Helpers
{
    /// <summary>
    /// Loads the manifest for the resend validation email controller
    /// </summary>
    internal class EmailValidationManifestFilter: IManifestFilter
    {
        public void Filter(List<PackageManifest> manifests)
        {
            manifests.Add(new PackageManifest
            {
                PackageName = "MediaWizards",
                Scripts = new []
                {
                    "/App_Plugins/MediaWizards/sendvalidation.controller.js"
                }
            });
        }
    }

    internal class ForumPostListManifestFilter: IManifestFilter
    {
        public void Filter(List<PackageManifest> manifests)
        {
            manifests.Add(new PackageManifest
            {
                PackageName = "ForumListView",
                Stylesheets = new []
                {
                    "/App_Plugins/ForumListView/layout.css"
                },
                Scripts = new []
                {
                    "/App_Plugins/ForumListView/forumposts.controller.js"
                }
            });
        }
    }
}
