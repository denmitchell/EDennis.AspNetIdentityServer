using IdentityModel.Client;
using IdentityServer4.Models;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new HttpClient();
            Task.Run(async () => {
                var disco = await GetDiscoveryDocument(client);

                var tokenResponse = await GetClientCredentialsTokenAsync(client, disco);
                Console.WriteLine(tokenResponse.AccessToken);

                var apiResult = await GetApiResult(client, tokenResponse);
                Console.WriteLine(apiResult);
            });
            Console.WriteLine("Press any key to stop");
            var _ = Console.ReadKey();
        }

        static async Task<string> GetApiResult(HttpClient client, TokenResponse tokenResponse) {
            // call api
            client.SetBearerToken(tokenResponse.AccessToken);

            var response = await client.GetAsync("https://localhost:5001/identity");
            if (!response.IsSuccessStatusCode) {
                return response.StatusCode.ToString();
            } else {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
        }


        static async Task<DiscoveryDocumentResponse> GetDiscoveryDocument(HttpClient client) {
            // discover endpoints from metadata
            var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5000");
            if (disco.IsError) {
                Console.WriteLine(disco.Error);
            }
            return disco;
        }

        static async Task<TokenResponse> GetClientCredentialsTokenAsync(HttpClient client, DiscoveryDocumentResponse disco) {
            // request token
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest {
                Address = disco.TokenEndpoint,

                ClientId = "client",
                ClientSecret = "secret",
                Scope = "api1"
            });

            if (tokenResponse.IsError) {
                Console.WriteLine(tokenResponse.Error);
            }

            return tokenResponse;
        }




    }
}
