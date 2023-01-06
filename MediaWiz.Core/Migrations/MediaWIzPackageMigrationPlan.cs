using System;
using Umbraco.Cms.Core.Packaging;

namespace MediaWiz.Forums.Migrations
{
    public class MediaWizPackageMigrationPlan : PackageMigrationPlan
    {
        public MediaWizPackageMigrationPlan()
            : base("MediaWiz Forums")
        {
        }

        protected override void DefinePlan()
        {
            From(String.Empty)
                .To<ImportPackageXmlMigration>(new Guid("AE51B566-D3AB-48AF-BF3B-6AF32408346E"));

        }
    }
}
