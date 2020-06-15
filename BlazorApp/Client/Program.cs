using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EDennis.AspNet.Base;
using System.Linq;

namespace BlazorApp.Client {
    public class Program {
        public static async Task Main(string[] args) {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddHttpClient("BlazorApp.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("BlazorApp.ServerAPI"));

            builder.Services.AddApiAuthorization();

            Apis apis = new Apis();
            Configuration.GetSection("Apis").Bind(apis);
            var oidcApi = apis.Single(a => a.Key == "OidcProvider").Value;

            builder.Services.AddOidcAuthentication(options => {
                options.ProviderOptions.Authority = $"https://{oidcApi.Host}:{oidcApi.HttpsPort}";
                options.ProviderOptions.ClientId = "BlazorApp.Client";
                options.ProviderOptions.ResponseType = "code";
                options.ProviderOptions.DefaultScopes.Add("roles");
            });



            await builder.Build().RunAsync();
        }
    }
}
