using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGRH.Web.Models;
using SGRH.Web.Services;
using System.Security.Claims;

namespace SGRH.Web.Controllers
{
    [Authorize]
    public class PersonalActionController : Controller
    {
        private readonly IPersonalActionService _personalActionService;
        private readonly IWarningService _warningService;

        public PersonalActionController(IPersonalActionService personalActionService, IWarningService warningService)
        {
            _personalActionService = personalActionService;
            _warningService = warningService;
        }

        public async Task<IActionResult> Index()
        {
           
            var userId = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.Notifications = await GetLatestNotifications();
            var personalActions = await _personalActionService.GetPersonalActionsForEmployee(userId);
            return View(personalActions);
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
    }
}
