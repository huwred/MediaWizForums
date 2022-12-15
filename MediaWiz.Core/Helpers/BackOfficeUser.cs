using System.Security.Claims;
using MediaWiz.Forums.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Extensions;

namespace MediaWiz.Forums.Helpers
{
    public class BackofficeUserAccessor : IBackofficeUserAccessor
    {
        private readonly IOptionsSnapshot<CookieAuthenticationOptions> _cookieOptionsSnapshot;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BackofficeUserAccessor(
            IOptionsSnapshot<CookieAuthenticationOptions> cookieOptionsSnapshot,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _cookieOptionsSnapshot = cookieOptionsSnapshot;
            _httpContextAccessor = httpContextAccessor;
        }


        public ClaimsIdentity BackofficeUser
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;

                if (httpContext == null)
                    return new ClaimsIdentity();

                CookieAuthenticationOptions cookieOptions = _cookieOptionsSnapshot.Get(Umbraco.Cms.Core.Constants.Security.BackOfficeAuthenticationType);
                string backOfficeCookie = httpContext.Request.Cookies[cookieOptions.Cookie.Name!];

                if (string.IsNullOrEmpty(backOfficeCookie))
                    return new ClaimsIdentity();

                AuthenticationTicket unprotected = cookieOptions.TicketDataFormat.Unprotect(backOfficeCookie!);
                if (unprotected == null)
                {
                    return new ClaimsIdentity();
                }
                ClaimsIdentity backOfficeIdentity = unprotected!.Principal.GetUmbracoIdentity();

                return backOfficeIdentity;
            }

        }
    }
}
