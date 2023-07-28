using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Scoping;

namespace MediaWiz.Forums.Composers
{
    /// <summary>
    /// Registers the View counter migration with Umbraco
    /// </summary>
    public class PostViewsComponent : IComponent
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IScopeAccessor _scopeAccessor;
        private readonly IMigrationBuilder _migrationBuilder;
        private readonly IKeyValueService _keyValueService;
        private readonly ILoggerFactory _logger;
        private readonly IRuntimeState _runtimeState;

        public PostViewsComponent(IScopeProvider scopeProvider,IScopeAccessor scopeAccessor, IMigrationBuilder migrationBuilder, IKeyValueService keyValueService, ILoggerFactory logger, IRuntimeState runtimeState)
        {
            _scopeProvider = scopeProvider;
            _scopeAccessor = scopeAccessor;
            _migrationBuilder = migrationBuilder;
            _keyValueService = keyValueService;
            _logger = logger;
            _runtimeState = runtimeState;
        }

        public void Initialize()
        {
            if (_runtimeState.Level < RuntimeLevel.Run)
            {
                return;
            }
            // Create a migration plan for a specific project/feature
            // We can then track that latest migration state/step for this project/feature
            var migrationPlan = new MigrationPlan("ForumHitCounter");

            // This is the steps we need to take
            // Each step in the migration adds a unique value
            migrationPlan.From(string.Empty)
                .To<ViewCounterTableMigration>("posthitcounter-db");

            // Go and upgrade our site (Will check if it needs to do the work or not)
            // Based on the current/latest step
            var upgrader = new Upgrader(migrationPlan);
            upgrader.Execute(new MigrationPlanExecutor(_scopeProvider,_scopeAccessor,_logger,_migrationBuilder), _scopeProvider, _keyValueService);
        }

        public void Terminate()
        {
        }
    }
}