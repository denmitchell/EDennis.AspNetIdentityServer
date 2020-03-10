using System.Collections.Generic;

namespace EDennis.AspNetIdentityServer.Models {
    public class AspNetClient {
        public string ClientId { get; set; }
        public ICollection<AspNetClientClaim> AspNetClientClaims { get; set; }
    }
}
