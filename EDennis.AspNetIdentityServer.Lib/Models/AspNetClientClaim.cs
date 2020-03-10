using System.Text.Json.Serialization;

namespace EDennis.AspNetIdentityServer.Models {
    public class AspNetClientClaim {
        public string ClientId { get; set; }
        public string ClaimType { get; set; }

        [JsonIgnore]
        public AspNetClient AspNetClient { get; set; }
    }
}
