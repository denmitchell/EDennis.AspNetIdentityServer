using EDennis.AspNetIdentityServer.Models;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EDennis.AspNetIdentityServer.Data {
    public class AspNetIdentityDbContext : ApiAuthorizationDbContext<AspNetIdentityUser> {
        public AspNetIdentityDbContext(
            DbContextOptions options,
            IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions) {
        }
    }
}
