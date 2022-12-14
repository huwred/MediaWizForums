using MediaWiz.Forums.Models;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Migrations;

namespace MediaWiz.Forums.Composers
{
    /// <summary>
    /// Creates View Counter table in Umbraco Database
    /// </summary>
    public class ViewCounterTableMigration : MigrationBase
    {
        public ViewCounterTableMigration(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            Logger.LogDebug("Running migration {MigrationStep}", "AddHitCounterTable");
            var storedproc = $@"

                CREATE PROCEDURE [dbo].[usp_record_view]
                (
                    @@node_id INT
                )
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS(SELECT 1 FROM ForumPostHitCounter WHERE node_id = @@node_id)
                    BEGIN
                        UPDATE tbl_hit_counter SET views = views + 1
                        WHERE node_id = @@node_id
                    END
                    ELSE
                    BEGIN
                        INSERT INTO ForumPostHitCounter(node_id, views) VALUES (@@node_id, 1)
                    END
                END
                GO

                CREATE PROCEDURE [dbo].[usp_get_view_count]
                (
                    @node_id INT
                )
                AS
                BEGIN

                    SET NOCOUNT ON;

                    SELECT views
                    FROM ForumPostHitCounter
                    WHERE node_id = @node_id  

                END

                GO";

            // Lots of methods available in the MigrationBase class - discover with this.
            if (TableExists("ForumPostHitCounter") == false)
            {
                Create.Table<ViewCounter>().Do();
                //Execute.Sql(storedproc);
            }
            else
            {

                Logger.LogDebug("The database table {DbTable} already exists, skipping", "ConfigTable");
            }
        }

    }
}