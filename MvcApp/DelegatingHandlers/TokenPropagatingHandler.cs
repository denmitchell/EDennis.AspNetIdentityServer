using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Globalization;
using IdentityServer4.Models;

namespace MvcApp {
    public class TokenPropagatingHandler<TApiClient> : DelegatingHandler {

        private readonly ILogger<TokenPropagatingHandler<TApiClient>> _logger;
        private readonly HttpClient _idpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Apis _apis = new Apis();
        public const int REFRESH_BUFFER = 30; //30 seconds
        public const string IDENTITY_SERVER_CLIENT_NAME = "IdentityServer";
        

        public TokenPropagatingHandler(IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            ILogger<TokenPropagatingHandler<TApiClient>> logger) {
            _logger = logger;
            _idpClient = httpClientFactory.CreateClient(IDENTITY_SERVER_CLIENT_NAME);
            _httpContextAccessor = httpContextAccessor;
            config.GetSection("Apis").Bind(_apis);
        }


        /// <summary>
        /// From https://github.com/KevinDockx/SecuringAspNetCore3WithOAuth2AndOIDC
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) {
            var accessToken = await GetAccessTokenAsync();

            if (!string.IsNullOrWhiteSpace(accessToken)) {
                request.SetBearerToken(accessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Adapted from https://github.com/KevinDockx/SecuringAspNetCore3WithOAuth2AndOIDC
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetAccessTokenAsync() {

            if (_httpContextAccessor == null)
                return null;

            if (_apis[typeof(TApiClient).Name].AccessTokenType == AccessTokenType.Reference)
                    return await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            //NEEDED ??? KEVIN DOES NOT APPEAR TO NEED THIS
            //if present, use the authorization code to obtain the access token
            var codeToken = await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.Code);
            if (codeToken != null)
                return await GetAccessTokenAsync(codeToken);


            //otherwise, if the authorization token hasn't expired, simply return the access token
            if (!(await NewTokenNeeded())) 
                return  await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            

            //otherwise, get the refresh token and build a new set of auth tokens for sign in
            var refreshResponse = await GetRefreshResponseAsync();
            var signInTokens = BuildSignInTokens(refreshResponse);

            // get authenticate result, containing the current principal & properties
            var currentAuthenticateResult = await _httpContextAccessor.HttpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            // store the updated tokens
            currentAuthenticateResult.Properties.StoreTokens(signInTokens);

            // sign in
            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                currentAuthenticateResult.Principal,
                currentAuthenticateResult.Properties);

            return refreshResponse.AccessToken;
        }


        private async Task<bool> NewTokenNeeded() {            
            if(DateTimeOffset.TryParse(await _httpContextAccessor.HttpContext.GetTokenAsync("expires_at"), out DateTimeOffset expiresAt)) {
                var threshold = DateTime.UtcNow.AddSeconds(-1 * REFRESH_BUFFER).ToUniversalTime();
                var expiration = expiresAt.ToUniversalTime();
                return expiration < threshold;                
            }
            return false;
        }

        private async Task<string> GetAccessTokenAsync(string codeToken) {
            // get the discovery document
            var disco = await _idpClient.GetDiscoveryDocumentAsync();
            var response = await _idpClient.RequestTokenAsync(
                new AuthorizationCodeTokenRequest {
                    Address = disco.TokenEndpoint,
                    ClientId = typeof(TApiClient).Name,
                    ClientSecret = _apis[typeof(TApiClient).Name].ClientSecret,
                    Code = codeToken
                }
            );
            return response.AccessToken;
        }


        private async Task<TokenResponse> GetRefreshResponseAsync() {

            // get the discovery document
            var disco = await _idpClient.GetDiscoveryDocumentAsync();

            // refresh the tokens
            var refreshToken = await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            var refreshResponse = await _idpClient.RequestRefreshTokenAsync(
                new RefreshTokenRequest {
                    Address = disco.TokenEndpoint,
                    ClientId = typeof(TApiClient).Name,
                    ClientSecret = _apis[typeof(TApiClient).Name].ClientSecret,
                    RefreshToken = refreshToken
                });

            return refreshResponse;
        }


        private IEnumerable<AuthenticationToken> BuildSignInTokens(TokenResponse refreshResponse) {
            // store the tokens             

            var updatedTokens = new List<AuthenticationToken>();
            updatedTokens.Add(new AuthenticationToken {
                Name = OpenIdConnectParameterNames.IdToken,
                Value = refreshResponse.IdentityToken
            });
            updatedTokens.Add(new AuthenticationToken {
                Name = OpenIdConnectParameterNames.AccessToken,
                Value = refreshResponse.AccessToken
            });
            updatedTokens.Add(new AuthenticationToken {
                Name = OpenIdConnectParameterNames.RefreshToken,
                Value = refreshResponse.RefreshToken
            });
            updatedTokens.Add(new AuthenticationToken {
                Name = "expires_at",
                Value = (DateTime.UtcNow + TimeSpan.FromSeconds(refreshResponse.ExpiresIn)).
                        ToString("o", CultureInfo.InvariantCulture)
            });

            return updatedTokens;
        }

    }
}
