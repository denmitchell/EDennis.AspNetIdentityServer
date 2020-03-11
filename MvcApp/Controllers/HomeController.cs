using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MvcApp.Models;

namespace MvcApp.Controllers {
    public class HomeController : Controller {
        private readonly ILogger<HomeController> _logger;
        private readonly Api1 _api;
        public HomeController(ILogger<HomeController> logger, Api1 api) {
            _logger = logger;
            _api = api;
        }

        public async Task<IActionResult> Index() {
            List<ClaimViewModel> claims = (await _api.GetClaimsFromApi1()).ToList();

            claims.Add(new ClaimViewModel { Type = "Get/Policy", Value = await _api.GetGetPolicy() });
            claims.Add(new ClaimViewModel { Type = "Get/Role", Value = await _api.GetGetRole() });
            claims.Add(new ClaimViewModel { Type = "Edit/Policy", Value = await _api.GetEditPolicy() });
            claims.Add(new ClaimViewModel { Type = "Edit/Role", Value = await _api.GetEditRole() });
            claims.Add(new ClaimViewModel { Type = "Delete/Policy", Value = await _api.GetDeletePolicy() });
            claims.Add(new ClaimViewModel { Type = "Delete/Role", Value = await _api.GetDeleteRole() });

            return View(claims);
        }



        public IActionResult Privacy() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
