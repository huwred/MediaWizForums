using MediaWiz.Forums.Models;

namespace MediaWiz.Forums.Interfaces
{
    public interface IViewCounterService
    {
        ViewCounter GetViewCount(int nodeId);
        void RecordView(int nodeId);
    }
}