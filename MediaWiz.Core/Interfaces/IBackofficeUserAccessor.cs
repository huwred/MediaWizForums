using System.Security.Claims;

namespace MediaWiz.Core.Interfaces
{
    public interface IBackofficeUserAccessor
    {
        ClaimsIdentity BackofficeUser { get; }
    }
}