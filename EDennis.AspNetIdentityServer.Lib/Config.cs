// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace EDennis.AspNetIdentityServer {
    public static class Config {
        public static IEnumerable<IdentityResource> Ids =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource(
                            "roles",
                                    "Your role(s)",
                            new List<string>() { "role" })
            };

        public static void ABC() {
            var i = new IdentityResource();
            i.UserClaims = null;
            return;

        }


        public static IEnumerable<ApiResource> Apis =>
            new List<ApiResource>
            {
                new ApiResource("Api1", "Api1", new List<string>() { "role" })
                {
                    ApiSecrets = { new Secret("secret".Sha256()) },
                    UserClaims = { "Name","Email","role","user_scope" }                     
                },
                new ApiResource("Api2", "Api2", new List<string>() { "role" })
                {
                    ApiSecrets = { new Secret("secret".Sha256()) },
                    UserClaims = { "Name","Email", "role", "user_scope" }
                }
            };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                // machine to machine client
                new Client
                {
                    ClientId = "client",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    // scopes that client has access to
                    AllowedScopes = { "Api1" }
                },
                // interactive ASP.NET Core MVC client
                new Client
                {
                    ClientId = "MvcApp",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,
                    RequireConsent = false,
                    RequirePkce = true,

                    AccessTokenType = AccessTokenType.Reference,

                    // where to redirect to after login
                    RedirectUris = { "https://localhost:5002/signin-oidc" },

                    // where to redirect to after logout
                    PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "Api1", "Api1.*.Get*", "Api1.*.Edit*", "Api1.*.Delete*",
                        "roles"
                    },

                    AllowOfflineAccess = true,
                    ClientClaimsPrefix = ""
                },
                // interactive ASP.NET Core MVC client
                new Client
                {
                    ClientId = "BlazorApp",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,
                    RequireConsent = false,
                    RequirePkce = true,

                    AccessTokenType = AccessTokenType.Reference,

                    // where to redirect to after login
                    RedirectUris = { "https://localhost:5003/signin-oidc" },

                    // where to redirect to after logout
                    PostLogoutRedirectUris = { "https://localhost:5003/signout-callback-oidc" },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "Api2", "Api2.*.Get*", "Api2.*.Edit*", "Api2.*.Delete*",
                        "roles"
                    },

                    AllowOfflineAccess = true,
                    ClientClaimsPrefix = ""
                }
            };
    }
}
