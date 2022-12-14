using System.Security.Claims;

namespace MediaWiz.Forums.Interfaces
{
    public interface IBackofficeUserAccessor
    {
        ClaimsIdentity BackofficeUser { get; }
    }
}