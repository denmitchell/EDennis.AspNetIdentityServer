using EDennis.AspNetIdentityServer.Data;
using EDennis.AspNetIdentityServer.Lib;
using EDennis.AspNetIdentityServer.Lib.Models;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EDennis.AspNetIdentityServer {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public static Regex RequestVerificationTokenRegEx = new Regex("(?<=<input\\s+name\\s*=\\s*\"__RequestVerificationToken\"\\s+type\\s*=\\s*\"hidden\"\\s+value\\s*=\\s*\")[A-Za-z0-9_-]+");

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {

            //Debugger.Launch();

            string cxnAspNetIdentity = Configuration["DbContexts:AspNetIdentityDbContext:ConnectionString"];
            services.AddDbContext<AspNetIdentityDbContext>(options =>
                options.UseSqlServer(cxnAspNetIdentity));

            services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<AspNetIdentityDbContext>()
                .AddDefaultTokenProviders();

            services.AddControllersWithViews();
            services.AddRazorPages();

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            string cxnConfiguration = Configuration["DbContexts:ConfigurationDbContext:ConnectionString"];
            string cxnPersistedGrant = Configuration["DbContexts:PersistedGrantDbContext:ConnectionString"];

            services.AddTransient<IEmailSender, MockEmailSender>();

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
            services.Replace(ServiceDescriptor.Transient<IProfileService, UserClientClaimsProfileService>());

            //services.AddScoped<IAuthenticationHandler,MockAuthenticationHandler>();

            //services.AddAuthentication(options => {
            //    options.AddScheme("MockAuthentication", configure => configure.HandlerType = typeof(MockAuthenticationHandler));
            //    options.DefaultAuthenticateScheme = "MockAuthentication";
            //    options.DefaultChallengeScheme = "MockAuthentication";
            //})
            //    .AddGoogle("Google", options => {
            //        options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
            //        options.ClientId = "<insert here>";
            //        options.ClientSecret = "<insert here>";
            //    })
            //    .AddMock("MockAuthentication", options => {
            //        options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
            //    });
                //.AddMicrosoftAccount("Microsoft", options=> {
                //    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                //    options.ClientId = "<insert here>";
                //    options.ClientSecret = "<insert here>";
                //})
                



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger) {

            app.Use(async (context, next) => {

                var url = UriHelper.GetDisplayUrl(context.Request);
                if (!url.Contains("/js/") && !url.Contains("/css/") && !url.Contains("/lib/") && !url.EndsWith(".ico")) {

                    context.Request.EnableBuffering();

                    var requestBodyStream = new MemoryStream();
                    var originalRequestBody = context.Request.Body;

                    await context.Request.Body.CopyToAsync(requestBodyStream);
                    requestBodyStream.Seek(0, SeekOrigin.Begin);

                    var requestBodyText = new StreamReader(requestBodyStream).ReadToEnd();
                    logger.LogInformation($@"
<REQUEST method = '{ context.Request.Method}'>
<URL><![CDATA[
{url}
]]></URL>
<HEADERS><![CDATA[
{GetHeaders(context.Request)}]]></HEADERS>
<BODY><![CDATA[
{requestBodyText}
]]></BODY>
</REQUEST>
");


                    requestBodyStream.Seek(0, SeekOrigin.Begin);
                    context.Request.Body = requestBodyStream;
                }

                if (!url.Contains("/js/") && !url.Contains("/css/") && !url.Contains("/lib/") && !url.EndsWith(".ico"))
                    await LogResponse(context, next, logger);
                else
                    await next.Invoke();


            });



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

            //app.Use(async (context, next) => {
            //    await context.ChallengeAsync("MockAuthentication");
            //    await next.Invoke();
            //});

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


        private string GetHeaders(HttpRequest req) {
            var sb = new StringBuilder();
            foreach (var hdr in req.Headers) {
                sb.AppendLine($"{hdr.Key}: {hdr.Value}");
            }
            return sb.ToString();
        }

        private string GetHeaders(HttpResponse resp) {
            var sb = new StringBuilder();
            foreach (var hdr in resp.Headers) {
                sb.AppendLine($"{hdr.Key}: {hdr.Value}");
            }
            return sb.ToString();
        }


        private async Task LogResponse(HttpContext context, Func<Task> next, ILogger<Startup> logger) {
            var originalBodyStream = context.Response.Body;
            var rmsm = new RecyclableMemoryStreamManager();
            await using var responseBody = rmsm.GetStream();
            context.Response.Body = responseBody;
            await next.Invoke();
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);


            if (context.Request.Path.Value.Contains("/Account/Login")) {
                text = RequestVerificationTokenRegEx.Match(text).Value;
                logger.LogInformation(@$"
<RESPONSE statusCode='{context.Response.StatusCode}'>{(context.Response.StatusCode == 302 ? "\n<LOCATION><![CDATA[" + context.Response.Headers["Location"].ToString() + "]]></LOCATION>" : "")}
<BODY>
  <__RequestVerificationToken>{text}</__RequestVerificationToken>
</BODY>
</RESPONSE>");
            } else {


                logger.LogInformation(@$"
<RESPONSE statusCode='{context.Response.StatusCode}'>
<HEADERS><![CDATA[
{GetHeaders(context.Response)}]]></HEADERS>
<BODY><![CDATA[
{text}
]]></BODY>
</RESPONSE>
");
            }
            await responseBody.CopyToAsync(originalBodyStream);
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
