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
                .To<ImportPackageXmlMigration>(new Guid("65060E59-E399-4A11-BB8C-270FC80DF316"))
                .To<PublishDocTypeChangesMigration>("DocType-Change");

        }
    }
}
