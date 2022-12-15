using System.Linq;
using MediaWiz.Forums.Interfaces;
using MediaWiz.Forums.Models;
using Umbraco.Cms.Infrastructure.Scoping;


namespace MediaWiz.Core.Services
{
    public class ForumViewCounterService : IViewCounterService
    {
        private readonly IScopeProvider _scopeProvider;
        public ForumViewCounterService(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }
        public ViewCounter GetViewCount(int nodeId)
        {
            using (var scope = _scopeProvider.CreateScope(autoComplete: true))
            {
                var database = scope.Database;

                var result = database.Query<ViewCounter>($@"SELECT * FROM ForumPostHitCounter WHERE node_id={nodeId}").FirstOrDefault();

                return result;
            }
        }

        /// <summary>
        /// Record a Topic view
        /// </summary>
        /// <param name="nodeId"></param>
        public void RecordView(int nodeId)
        {
            var foundIt = GetViewCount(nodeId);


            ViewCounter config = new ViewCounter
            {
                NodeId = nodeId,
                Views = 1
            };
            using (var scope = _scopeProvider.CreateScope())
            {
                var database = scope.Database;
                if (foundIt != null)
                {
                    foundIt.Views += 1;
                    database.ExecuteScalar<ViewCounter>($@"UPDATE ForumPostHitCounter SET Views = {foundIt.Views} WHERE node_id={foundIt.NodeId}");
                }
                else
                {
                    database.ExecuteScalar<ViewCounter>($@"INSERT INTO ForumPostHitCounter VALUES({config.NodeId},{config.Views})");
                }
                
                scope.Complete();
            }
        }

    }


}