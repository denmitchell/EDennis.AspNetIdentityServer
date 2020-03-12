using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BlazorApp.Data;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Net.Http.Headers;
using IdentityModel.Client;
using System.Net.Http;

namespace BlazorApp {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            
            services.AddScoped<IIdentityService,IdentityService>();

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            Apis apis = new Apis();
            Configuration.GetSection("Apis").Bind(apis);

            services.AddSingleton<IDiscoveryCache>(r => {
                var factory = r.GetRequiredService<IHttpClientFactory>();
                return new DiscoveryCache(apis["IdentityServer"].Url, () => factory.CreateClient());
            });

            services.AddHttpContextAccessor();
            services.AddTransient<TokenPropagatingHandler<Api2>>();

            // create an HttpClient used for accessing the API
            services.AddHttpClient("Api2", client => {
                client.BaseAddress = apis["Api2"].Uri;
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            }).AddHttpMessageHandler<TokenPropagatingHandler<Api2>>();

            // create an HttpClient used for accessing the IDP
            services.AddHttpClient("IdentityServer", client => {
                client.BaseAddress = apis["IdentityServer"].Uri;
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });

            services.AddScoped<Api2>();


            services.AddAuthentication(options => {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options => {
                    options.Authority = "https://localhost:5000";
                    options.RequireHttpsMetadata = false;
                    options.ClientId = "BlazorApp";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";
                    options.Scope.Add("Api2");
                    options.Scope.Add("roles");
                    options.SaveTokens = true;
                });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints => {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
