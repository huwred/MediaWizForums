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

            To<ImportPackageXmlMigration>(new Guid("87B98517-9A06-457D-9445-59F56CFD1A32"));
        }
    }
}
