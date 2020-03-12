using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Api1;
using IdentityModel.Client;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using MvcApp;

namespace Api {
    public class Startup {

        IConfiguration Configuration;

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) {
            services.AddControllers();

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options => {
                    options.Authority = "https://localhost:5000";
                    options.ApiName = "Api1";
                    options.ApiSecret = "secret";
                });

            services.AddAuthorization(
            options =>
            {
                options.AddPolicy("GetPolicy", policy =>
                    policy.RequireClaim("Api1.scope", "Api1.*.Get*"));
                options.AddPolicy("EditPolicy", policy =>
                    policy.RequireClaim("Api1.scope", "Api1.*.Edit*"));
                options.AddPolicy("DeletePolicy", policy =>
                    policy.RequireClaim("Api1.scope", "Api1.*.Delete*"));
            });


            //services.AddAuthentication("Bearer")
            //    .AddJwtBearer("Bearer", options => {
            //        options.Authority = "https://localhost:5000";
            //        options.RequireHttpsMetadata = false;

            //        options.Audience = "api1";
            //    });



            Apis apis = new Apis();
            Configuration.GetSection("Apis").Bind(apis);

            services.AddSingleton<IDiscoveryCache>(r => {
                var factory = r.GetRequiredService<IHttpClientFactory>();
                return new DiscoveryCache(apis["IdentityServer"].Url, () => factory.CreateClient());
            });

            services.AddHttpContextAccessor();


            // create an HttpClient used for accessing the IDP
            services.AddHttpClient("IdentityServer", client => {
                client.BaseAddress = apis["IdentityServer"].Uri;
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });

            services.AddScoped<IdentityServer>();



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
