using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace EDennis.AspNetIdentityServer.Lib {
    public class MockAuthenticationHandler : OpenIdConnectHandler, IAuthenticationHandler {
        const string userId = "larry@stooges.org";
        const string userName = "larry@stooges.org";
        const string userRole = "User";

        public MockAuthenticationHandler(
          IOptionsMonitor<OpenIdConnectOptions> options,
          ILoggerFactory logger,
          HtmlEncoder htmlEncoder,
          UrlEncoder urlEncoder,
          ISystemClock clock)
          : base(options, logger, htmlEncoder, urlEncoder, clock) { }


        protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
            var claims = new[]
              {
          new Claim(ClaimTypes.NameIdentifier, userId),
          new Claim(JwtClaimTypes.Subject,"6ba0718b-28fd-4e7f-9927-236308859e54"),
          new Claim(JwtClaimTypes.Name,userName),
          new Claim(ClaimTypes.Name, userName),
          new Claim(ClaimTypes.Role, userRole),
          new Claim(ClaimTypes.Email, "larry@stooges.org"),
          new Claim("idp","local")
        };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return await Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }


    public static class AuthenticationBuilderExtensions_MockAuthenticationHandler {
        public static AuthenticationBuilder AddMock(this AuthenticationBuilder builder, 
            string authenticationName, Action<OAuthOptions> options) {

            return builder;
        }
    }

}
