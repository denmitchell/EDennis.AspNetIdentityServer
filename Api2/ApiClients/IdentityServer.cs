using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api1 {
    public class IdentityServer {

        private readonly HttpClient _client;
        private readonly HttpContext _httpContext;
        public IdentityServer(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor) {
            _client = httpClientFactory.CreateClient("IdentityServer");
            _httpContext = httpContextAccessor.HttpContext;
        }

        public async Task<IEnumerable<ClaimViewModel>> GetClaims() {

            var disco = await _client.GetDiscoveryDocumentAsync();
            var accessToken = await _httpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            var response = await _client.IntrospectTokenAsync(new TokenIntrospectionRequest {
                Address = disco.IntrospectionEndpoint,
                ClientId = "api2",
                ClientSecret = "secret",

                Token = accessToken
            });

            return response.Claims.Select(c => new ClaimViewModel { Type = $"USER::{c.Type}", Value = c.Type });

        }
    }
}
