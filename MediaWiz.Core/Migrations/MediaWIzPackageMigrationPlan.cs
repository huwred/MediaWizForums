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
            From("65060e59-e399-4a11-bb8c-270fc80df316")
                .To<PublishRootBranchPostMigration>(new Guid("E8086ABC-CC18-4620-9AED-AC97EF49094F"));
            From("E8086ABC-CC18-4620-9AED-AC97EF49094F")
                .To<PublishApprovalChangesMigration>(new Guid("32B14FE8-7748-423C-B48D-ACCDD84C447D"));
        }
    }
}
