using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SGRH.Web.Models;
using SGRH.Web.Services;
using SGRH.Web.Models.Entities;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SGRH.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly IWarningService _warningService;

        public HomeController(SignInManager<User> signInManager, IWarningService warningService)
        {
            _signInManager = signInManager;
            _warningService = warningService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Titulo = "Dashboard";
            if (!_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.Notifications = await GetLatestNotifications();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<List<WarningViewModel>> GetLatestNotifications()
        {
            var currentUserWarnings = await _warningService.GetLatestNotifications(User);

            var latestNotifications = new List<WarningViewModel>();
            foreach (var warning in currentUserWarnings)
            {
                var notification = new WarningViewModel
                {
                    Id_Warnings = warning.Id_Warnings,
                    Reason = warning.Reason,
                    Observations = warning.Observations
                };
                latestNotifications.Add(notification);
            }

            return latestNotifications;
        }

        public IActionResult SupervisorDashboard()
        {
            return View();
        }

        public IActionResult RhDashboard()
        {
            return View();
        }
    }
}
