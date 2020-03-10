using EDennis.AspNetIdentityServer.Data;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.AspNetIdentityServer.Lib.Models {
    public class UserClientClaimsProfileService : IProfileService {

        private readonly AspNetIdentityDbContext _dbContext;

        public UserClientClaimsProfileService(AspNetIdentityDbContext dbContext) {
            _dbContext = dbContext;
        }

        public Task GetProfileDataAsync(ProfileDataRequestContext context) {

            var clientId = context.Client.ClientId;
            var userId = context.Subject.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type.EndsWith("NameIdentifier"))?.Value;

            if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(userId)) {
                var claims = _dbContext.AspNetClaims
                    .FromSqlInterpolated($"exec GetUserClientClaims {context.Subject.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type.EndsWith("NameIdentifier"))}, {context.Client.ClientId}")
                    .Select(c=>new Claim(c.ClaimType,c.ClaimValue))
                    .ToList();
                context.IssuedClaims.AddRange(claims);
            }
            return Task.CompletedTask;

        }

        public Task IsActiveAsync(IsActiveContext context) {
            context.IsActive = true;
            return Task.FromResult(true);
        }
    }
}
