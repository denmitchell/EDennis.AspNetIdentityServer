using EDennis.AspNetIdentityServer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EDennis.AspNetIdentityServer.Data {
    public class AspNetIdentityDbContext : IdentityDbContext {
        public AspNetIdentityDbContext(DbContextOptions<AspNetIdentityDbContext> options)
            : base(options) {
        }

        public DbSet<AspNetClaim> AspNetClaims { get; set; }
        public DbSet<AspNetClient> AspNetClients { get; set; }
        public DbSet<AspNetClientClaim> AspNetClientClaims { get; set; }


        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

            builder.Entity<AspNetClient>(e => {
                e.ToTable("AspNetClients");
                e.HasKey(x => x.ClientId);
            });

            builder.Entity<AspNetClaim>(e => {
                e.ToTable("AspNetClaims");
                e.HasKey(x => new { x.ClaimType, x.ClaimValue });
            });

            builder.Entity<AspNetClientClaim>(e => {
                e.ToTable("AspNetClientClaims");
                e.HasKey(x => new { x.ClientId, x.ClaimType });
                e.HasIndex(x => x.ClaimType).IsUnique(false);
                e.HasOne(x => x.AspNetClient)
                    .WithMany(r => r.AspNetClientClaims)
                    .HasForeignKey("fk_AspNetClientClaims_AspNetClient");
            });
        }
    }
}
