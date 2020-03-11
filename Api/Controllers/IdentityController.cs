using Api1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers {
    [Route("identity")]
    [Authorize]
    public class IdentityController : ControllerBase {

        private readonly IdentityServer _idp;

        public IdentityController(IdentityServer idp) {
            _idp = idp;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Get() {
            var claims = User.Claims.Select(c => new ClaimViewModel { Type = c.Type, Value = c.Value }).ToList();
            claims.Add(new ClaimViewModel { Type = "UserName", Value = User.Identity.Name });

            var userClaims = await _idp.GetClaims();

            claims.AddRange(userClaims);

            return new JsonResult(claims);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("Delete/Role")]
        public IActionResult DeleteRole() {
            return new JsonResult("{\"Action\":\"Delete/Role\"}");
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet("Edit/Role")]
        public IActionResult EditRole() {
            return new JsonResult("{\"Action\":\"Edit/Role\"}");
        }

        [Authorize(Roles = "Admin,User,Readonly")]
        [HttpGet("Get/Role")]
        public IActionResult GetRole() {
            return new JsonResult("{\"Action\":\"Get/Role\"}");
        }


        [Authorize(Policy = "DeletePolicy")]
        [HttpGet("Delete/Policy")]
        public IActionResult DeletePolicy() {
            return new JsonResult("{\"Action\":\"Delete/Policy\"}");
        }

        [Authorize(Policy = "EditPolicy")]
        [HttpGet("Edit/Policy")]
        public IActionResult EditPolicy() {
            return new JsonResult("{\"Action\":\"Edit/Policy\"}");
        }

        [Authorize(Policy = "GetPolicy")]
        [HttpGet("Get/Policy")]
        public IActionResult GetPolicy() {
            return new JsonResult("{\"Action\":\"Get/Policy\"}");
        }


    }
}
