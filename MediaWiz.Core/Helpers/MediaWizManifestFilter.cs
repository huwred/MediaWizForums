using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Manifest;

namespace MediaWiz.Forums.Helpers
{
    internal class MediaWizManifestFilter: IManifestFilter
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
}
