using System;
using Umbraco.Cms.Core.Packaging;

namespace MediaWiz.Forums.Migrations
{
    public class MediaWizPackageMigrationPlan : PackageMigrationPlan
    {

        public override bool IgnoreCurrentState => true;

        public MediaWizPackageMigrationPlan()
            : base("MediaWiz Forums")
        {
        }

        protected override void DefinePlan()
        {
            From(String.Empty)
                .To<ImportPackageXmlMigration>(new Guid("65060e59-e399-4a11-bb8c-270fc80df316"));
            //From(String.Empty)
            //    .To<PublishDocTypeChangesMigration>("DocType-Change");

        }
    }
}
