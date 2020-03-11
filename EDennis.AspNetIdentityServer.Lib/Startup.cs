using EDennis.AspNetIdentityServer.Data;
using EDennis.AspNetIdentityServer.Lib;
using EDennis.AspNetIdentityServer.Lib.Models;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace EDennis.AspNetIdentityServer {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {

            //Debugger.Launch();

            string cxnAspNetIdentity = Configuration["DbContexts:AspNetIdentityDbContext:ConnectionString"];
            services.AddDbContext<AspNetIdentityDbContext>(options =>
                options.UseSqlServer(cxnAspNetIdentity));
            
            services.AddIdentity<IdentityUser,IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<AspNetIdentityDbContext>();

            services.AddControllersWithViews();
            services.AddRazorPages();

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            string cxnConfiguration = Configuration["DbContexts:ConfigurationDbContext:ConnectionString"];
            string cxnPersistedGrant = Configuration["DbContexts:PersistedGrantDbContext:ConnectionString"];

            services.AddIdentityServer()
                .AddConfigurationStore(options => {
                    options.ConfigureDbContext = b => b.UseSqlServer(cxnConfiguration,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddOperationalStore(options => {
                    options.ConfigureDbContext = b => b.UseSqlServer(cxnPersistedGrant,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddDeveloperSigningCredential()
                .AddAspNetIdentity<IdentityUser>();

                //replace Identity Server's ProfileService with a profile service that determines
                //which claims to retrieve for a user/client as configured in the database
                //services.Replace(ServiceDescriptor.Transient<IProfileService, UserClientClaimsProfileService>());

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            } else {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();
            //app.UseAuthentication(); //UseIdentityServer calls this
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }


        // Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
        // Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
        private void InitializeDatabase(IApplicationBuilder app) {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope()) {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
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
            }
        }
    }
}
