using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace MediaWiz.Forums.Models
{
    [TableName("ForumPostHitCounter")]
    [PrimaryKey("NodeId", AutoIncrement = false)]
    public class ViewCounter
    {

        [Column("Node_Id")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public int NodeId { get; set; }

        [Column("Views")]
        public int Views { get; set; }

    }
}