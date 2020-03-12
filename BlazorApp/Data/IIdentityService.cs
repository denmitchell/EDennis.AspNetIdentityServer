using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorApp.Data {
    public interface IIdentityService {
        Task<IEnumerable<ClaimViewModel>> GetClaims();
        Task<string> GetDeletePolicy();
        Task<string> GetDeleteRole();
        Task<string> GetEditPolicy();
        Task<string> GetEditRole();
        Task<string> GetGetPolicy();
        Task<string> GetGetRole();
    }
}