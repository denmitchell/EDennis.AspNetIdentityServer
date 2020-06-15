using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Data {
    public class IdentityService : IIdentityService {

        private readonly Api2 _api;

        public IdentityService(Api2 api) {
            _api = api;
        }

        public async Task<IEnumerable<ClaimViewModel>> GetClaims()
            => await _api.GetClaimsFromApi2();

        public async Task<string> GetGetPolicy() => await _api.GetGetPolicy();
        public async Task<string> GetEditPolicy() => await _api.GetEditPolicy();
        public async Task<string> GetDeletePolicy() => await _api.GetDeletePolicy();
        public async Task<string> GetGetRole() => await _api.GetGetRole();
        public async Task<string> GetEditRole() => await _api.GetEditRole();
        public async Task<string> GetDeleteRole() => await _api.GetDeleteRole();


    }
}
