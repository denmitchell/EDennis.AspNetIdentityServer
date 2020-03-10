using EDennis.AspNetIdentityServer.Data;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Linq;
using System.Security.Claims;

namespace EDennis.AspNetIdentityServer {
    public class Program {
        public static int Main(string[] args) {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Literate)
                .CreateLogger();

            try {
                Log.Information("Starting host...");
                var host = CreateHostBuilder(args).Build();

                //seed the database if /seed argument provided
                if (args.Contains("/seed")) {
                    CreateAndSeedUserDatabase(host);
                    CreateAndSeedIdentityDatabase(host);
                    args = args.Except(new[] { "/seed" }).ToArray();
                }

                //run the app
                host.Run();
            } catch (Exception ex) {
                Log.Fatal(ex, "Host terminated unexpectedly.");
            }

            return 0;



        }

        private static void CreateAndSeedIdentityDatabase(IHost host) {
            //seed the database
            using var scope = host.Services.CreateScope();
            try {
                scope.ServiceProvider
                    .GetRequiredService<PersistedGrantDbContext>()
                    .Database
                    .Migrate();

                var context = scope.ServiceProvider
                    .GetRequiredService<ConfigurationDbContext>();

                //ensure db is migrated before seeding
                context.Database.Migrate();

                if (!context.Clients.Any()) {
                    foreach (var client in Config.Clients) {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any()) {
                    foreach (var resource in Config.Ids) {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any()) {
                    foreach (var resource in Config.Apis) {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }


            } catch (Exception ex) {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the Identity database.");
            }
        }



        private static void CreateAndSeedUserDatabase(IHost host) {
            //seed the database
            using var scope = host.Services.CreateScope();
            try {
                var context = scope.ServiceProvider.GetService<AspNetIdentityDbContext>();

                //ensure db is migrated before seeding
                context.Database.Migrate();

                //use the user manager to create test users
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                var jack = userManager.FindByNameAsync("Jack").Result;
                if (jack == null) {
                    jack = new IdentityUser {
                        UserName = "Jack",
                        EmailConfirmed = true
                    };

                    var result = userManager.CreateAsync(jack, "P@ssword1").Result;
                    if (!result.Succeeded)
                        throw new Exception(result.Errors.First().Description);

                    result = userManager.AddClaimsAsync(jack, new Claim[] {
                                new Claim(JwtClaimTypes.Name, "Jack Torrence"),
                                new Claim(JwtClaimTypes.GivenName, "Jack"),
                                new Claim(JwtClaimTypes.FamilyName, "Torrence"),
                                new Claim(JwtClaimTypes.Email, "jack.torrence@email.com"),
                                new Claim("country", "BE")
                            }).Result;

                    if (!result.Succeeded)
                        throw new Exception(result.Errors.First().Description);
                }

                var wendy = userManager.FindByNameAsync("Wendy").Result;
                if (wendy == null) {
                    wendy = new IdentityUser {
                        UserName = "Wendy",
                        EmailConfirmed = true
                    };

                    var result = userManager.CreateAsync(wendy, "P@ssword1").Result;
                    if (!result.Succeeded)
                        throw new Exception(result.Errors.First().Description);

                    result = userManager.AddClaimsAsync(wendy, new Claim[] {
                                new Claim(JwtClaimTypes.Name, "Wendy Torrence"),
                                new Claim(JwtClaimTypes.GivenName, "Wendy"),
                                new Claim(JwtClaimTypes.FamilyName, "Torrence"),
                                new Claim(JwtClaimTypes.Email, "wendy.torrence@email.com"),
                                new Claim("country", "NL")
                            }).Result;

                    if (!result.Succeeded)
                        throw new Exception(result.Errors.First().Description);
                }


            } catch (Exception ex) {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the User database.");
            }
        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
