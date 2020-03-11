NOTE: still working on how to get user claims from reference token


Per https://github.com/KevinDockx/SecuringAspNetCore3WithOAuth2AndOIDC ...
1. In Api startup ...
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options => {
                    options.Authority = "https://localhost:5000";
                    options.ApiName = "api1";
                    options.ApiSecret = "secret";
                });

2. In Identity Server Config ...
        public static IEnumerable<ApiResource> Apis =>
            new List<ApiResource>
            {
                new ApiResource("api1", "Api1", new List<string>() { "role" })
                {
                    ApiSecrets = { new Secret("secret".Sha256()) }
                }
            };

    ...

        new Client
        {
            ClientId = "mvc",
            ClientSecrets = { new Secret("secret".Sha256()) },

            AllowedGrantTypes = GrantTypes.Code,
            //RequireConsent = false,
            RequirePkce = true,
            UpdateAccessTokenClaimsOnRefresh = true,

            AccessTokenType = AccessTokenType.Reference,

3. In Mvc Startup ...


            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            Apis apis = new Apis();
            Configuration.GetSection("Apis").Bind(apis);

            services.AddSingleton<IDiscoveryCache>(r => {
                var factory = r.GetRequiredService<IHttpClientFactory>();
                return new DiscoveryCache(apis["IdentityServer"].Url, () => factory.CreateClient());
            });

            services.AddHttpContextAccessor();
            services.AddTransient<TokenPropagatingHandler<Api1>>();

            // create an HttpClient used for accessing the API
            services.AddHttpClient("Api1", client => {
                client.BaseAddress = apis["Api1"].Uri;
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            }).AddHttpMessageHandler<TokenPropagatingHandler<Api1>>();

            // create an HttpClient used for accessing the IDP
            services.AddHttpClient("IdentityServer", client => {
                client.BaseAddress = apis["IdentityServer"].Uri;
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });

            services.AddScoped<Api1>();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
                .AddCookie("Cookies")
                .AddOpenIdConnect("oidc", options => {
                    options.Authority = "https://localhost:5000";
                    options.RequireHttpsMetadata = false;

                    options.ClientId = "mvc";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";
                    options.Scope.Add("api1");
                    options.Scope.Add("roles");
                    options.SaveTokens = true;
                });

4. Include TokenPropagatingHandler.cs

5. In appsettings ...

  "Apis": {
    "IdentityServer": {
      "Host": "localhost",
      "HttpsPort": 5000,
      "ClientSecret": "secret",
      "AccessTokenType": "Jwt"
    },
    "Api1": {
      "Host": "localhost",
      "HttpsPort": 5001,
      "ClientSecret": "secret",
      "AccessTokenType":  "Reference"
    }
  },
