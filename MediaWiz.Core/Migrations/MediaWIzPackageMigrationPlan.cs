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
                .To<ImportPackageXmlMigration>(new Guid("C701A4AC-BC7F-42BB-A8E6-6B0FA1F013AF"));

        }
    }
}
