using MediaWiz.Core.Models;

namespace MediaWiz.Core.Interfaces
{
    public interface IViewCounterService
    {
        ViewCounter GetViewCount(int nodeId);
        void RecordView(int nodeId);
    }
}