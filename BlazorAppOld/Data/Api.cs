using System;
using System.Collections.Generic;
using IdentityServer4.Models;

namespace BlazorApp {
    public class Apis : Dictionary<string, Api> { }
    public class Api {
        public string Host { get; set; }
        public int HttpsPort { get; set; }
        public string ClientSecret { get; set; }
        public AccessTokenType AccessTokenType {get; set;}
        public string Url => $"https://{Host}:{HttpsPort}";
        public Uri Uri => new Uri(Url);
    }
}
