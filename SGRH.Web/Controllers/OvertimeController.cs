using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SGRH.Web.Enums;
using SGRH.Web.Models;
using SGRH.Web.Models.Entities;
using SGRH.Web.Models.ViewModels;
using SGRH.Web.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SGRH.Web.Controllers
{
    [Authorize]
    public class OvertimeController : Controller
    {
        private readonly IPersonalActionService _personalActionService;
        private readonly IOvertimeService _overtimeService;
        private readonly UserManager<User> _userManager;
        private readonly IWarningService _warningService;
        private readonly IServiceUser _serviceUser;

        public OvertimeController(IPersonalActionService personalActionService, IOvertimeService overtimeService, UserManager<User> userManager, IWarningService warningService, IServiceUser serviceUser)
        {
            _personalActionService = personalActionService;
            _overtimeService = overtimeService;
            _userManager = userManager;
            _warningService = warningService;
            _serviceUser = serviceUser;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            ViewBag.Titulo = "Gestión de Horas Extra";
            ViewBag.NombreUbicacion = "Lista de Solicitudes";
            ViewBag.Notifications = await GetLatestNotifications();
            var overtimes = await _overtimeService.GetOvertimes(User);
            return View(overtimes);
        }

        public async Task<IActionResult> MyOvertimes()
        {
            ViewBag.Titulo = "Gestión de Horas Extra";
            ViewBag.NombreUbicacion = "Lista de Solicitudes";
            ViewBag.Notifications = await GetLatestNotifications();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Usuario no identificado, debe iniciar sesión para ver sus horas extra.";
                return RedirectToAction("Login", "Account");
            }

            var myOvertimes = await _overtimeService.GetMyOvertimes(userId);
            return View(myOvertimes);
        }


        [Authorize]
        public async Task<IActionResult> RequestOvertime()
        {
            // Obtener la jornada laboral del usuario
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _serviceUser.GetUserById(userId);
            var workPeriod = user.workPeriod;

            if (workPeriod != null)
            {

                ViewBag.MaxDailyOTHours = workPeriod.MaxDailyOTHours;
            }
            else
            {

                TempData["ErrorMessage"] = "La jornada laboral del usuario no está definida.";
                return RedirectToAction("MyOvertimes");
            }
            ViewBag.Titulo = "Gestión de Horas Extra";
            ViewBag.NombreUbicacion = "Solicitud de Horas Extra";
            ViewBag.Notifications = await GetLatestNotifications();
            return View(new OvertimeViewModel());
        }


        [HttpPost]
        public async Task<IActionResult> RequestOvertime(OvertimeViewModel model)
        {
            ViewBag.Notifications = await GetLatestNotifications();
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _serviceUser.GetUserById(userId);

                if (userId == null)
                {
                    return RedirectToAction("MyOvertimes");
                }

                var personalAction = await _personalActionService.CreatePersonalAction(ActionType.Horas_Extra, model.Description, user.Id);

                var workPeriod = user.workPeriod;
                if (workPeriod is null)
                {
                    TempData["ErrorMessage"] = "La jornada laboral del usuario no está definida.";
                    return RedirectToAction("MyOvertimes");
                }

                decimal salaryPerHour;
     
                if (workPeriod.PeriodName == "Jornada Diurna")
                {
                    salaryPerHour = user.BaseSalary / 30 / 8 * 1.5m;

                }
                else if (workPeriod.PeriodName == "Jornada Mixta")
                {
                    salaryPerHour = user.BaseSalary / 30 / 7 * 1.5m;

                }
                else
                {
                    salaryPerHour = user.BaseSalary / 30 / 6 / 1.5m;
                }

                var (success, message) = await _overtimeService.CreateOvertime(personalAction, model.OT_Date, model.Hours_Worked, model.TypeOT, salaryPerHour);
                if (success)
                    TempData["SuccessMessage"] = "Se registró la solicitud exitosamente";
                else if (!string.IsNullOrEmpty(message))
                    TempData["ErrorMessage"] = message;
                else
                    TempData["ErrorMessage"] = "Error al solicitar su solicitud de horas extra.";

                return RedirectToAction("MyOvertimes");
            }
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> UpdateOvertime(int id)
        {
            // Obtener la jornada laboral del usuario
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _serviceUser.GetUserById(userId);
            var workPeriod = user.workPeriod;

            if (workPeriod != null)
            {
                ViewBag.MaxDailyOTHours = workPeriod.MaxDailyOTHours;
            }
            else
            {
                TempData["ErrorMessage"] = "La jornada laboral del usuario no está definida.";
                return RedirectToAction("MyOvertimes");
            }

            // Obtener la instancia de horas extra existente
            var overtime = await _overtimeService.GetOvertimeById(id);
            if (overtime == null)
            {
                TempData["ErrorMessage"] = "No se encontró el registro de horas extra para actualizar.";
                return RedirectToAction("MyOvertimes");
            }

            // Convertir el objeto Overtime a un objeto OvertimeViewModel
            var overtimeViewModel = new OvertimeViewModel
            {
                OT_Date = overtime.OT_Date,
                Hours_Worked = overtime.Hours_Worked,
                Description = overtime.PersonalAction?.Description,
                TypeOT = overtime.TypeOT
            };

            ViewBag.Titulo = "Gestión de Horas Extra";
            ViewBag.NombreUbicacion = "Actualización de Horas Extra";
            ViewBag.Notifications = await GetLatestNotifications();

            // Pasar la instancia de OvertimeViewModel a la vista
            return View(overtimeViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateOvertime(int id, OvertimeViewModel model)
        {
            ViewBag.Notifications = await GetLatestNotifications();
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _serviceUser.GetUserById(userId);

                if (userId == null)
                {
                    return RedirectToAction("MyOvertimes");
                }

                // Obtén la hora extra existente
                var overtime = await _overtimeService.GetOvertimeById(id);
                if (overtime == null)
                {
                    TempData["ErrorMessage"] = "No se encontró el registro de horas extra para actualizar.";
                    return RedirectToAction("MyOvertimes");
                }

                // Actualiza la PersonalAction existente en lugar de crear una nueva
                var personalAction = overtime.PersonalAction;
                personalAction.Description = model.Description;

                await _personalActionService.UpdatePersonalAction(personalAction);

                var workPeriod = user.workPeriod;
                if (workPeriod is null)
                {
                    TempData["ErrorMessage"] = "La jornada laboral del usuario no está definida.";
                    return RedirectToAction("MyOvertimes");
                }

                decimal salaryPerHour;

                if (workPeriod.PeriodName == "Jornada Diurna")
                {
                    salaryPerHour = user.BaseSalary / 30 / 8 * 1.5m; 

                }
                else if (workPeriod.PeriodName == "Jornada Mixta")
                {
                    salaryPerHour = user.BaseSalary / 30 / 7 * 1.5m; 

                }
                else
                {
                    salaryPerHour = user.BaseSalary / 30 / 6 / 1.5m; 
                }

                var (success, message) = await _overtimeService.UpdateOvertime(id, personalAction, model.OT_Date, model.Hours_Worked, model.TypeOT, salaryPerHour);
                if (success)
                    TempData["SuccessMessage"] = "Se actualizó la solicitud exitosamente";
                else if (!string.IsNullOrEmpty(message))
                    TempData["ErrorMessage"] = message;
                else
                    TempData["ErrorMessage"] = "Error al actualizar su solicitud de horas extra.";

                return RedirectToAction("MyOvertimes");
            }
            return View(model);
        }



        public async Task<IActionResult> OvertimeManagement()
        {
            ViewBag.Titulo = "Gestión de Horas Extra";
            ViewBag.NombreUbicacion = "Gestión de horas extra";
            ViewBag.Notifications = await GetLatestNotifications();
            var pendingOvertimes = await _overtimeService.OvertimeRequests(Status.Pendiente, User);

            return View(pendingOvertimes);
        }

        [Authorize(Roles = "Administrador, SupervisorRH, SupervisorDpto")]
        [HttpPost]
        public async Task<IActionResult> Approve(int overtimeId)
        {
            ViewBag.Notifications = await GetLatestNotifications();
            var (result, message) = await _overtimeService.ApproveOvertimeRequest(overtimeId, User);

            if (result)
            {
                TempData["SuccessMessage"] = "Solicitud aprobada exitosamente.";
                RedirectToAction("OvertimeManagement");
            }
            else if (!string.IsNullOrEmpty(message))
            {
                TempData["ErrorMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Error inesperado al aprobar la solicitud de horas extra.";
                return RedirectToAction("OvertimeManagement");
            }

            return RedirectToAction("OvertimeManagement");
        }
        [Authorize(Roles = "Administrador, SupervisorRH, SupervisorDpto")]
        [HttpPost]
        public async Task<IActionResult> Reject(int overtimeId)
        {
            ViewBag.Notifications = await GetLatestNotifications();
            var (result, message) = await _overtimeService.RejectOvertimeRequest(overtimeId, User);

            if (result)
                TempData["SuccessMessage"] = "Solicitud rechazada exitosamente.";
            else if (!string.IsNullOrEmpty(message))
                TempData["ErrorMessage"] = message;
            else
                TempData["ErrorMessage"] = "Error inesperado al rechazar la solicitud de horas extra.";
            return RedirectToAction("OvertimeManagement");
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

        public async Task<JsonResult> GetOvertimes(string userId)
        {
            var otCount = await _overtimeService.GetOvertimeCount(userId);

            return Json(otCount);
        }

        public async Task<JsonResult> GetOvertimesAmount(string userId)
        {
            var otAmount = await _overtimeService.GetTotalApprovedOvertimeAmount(userId);

            return Json(otAmount);
        }

        [HttpGet]
        public async Task<JsonResult> GetMonthlyOvertimeAmount(string userId)
        {
            var monthlyOvertimeAmounts = await _overtimeService.GetMonthlyOvertimeAmount(userId);
            var formattedData = monthlyOvertimeAmounts.Select(a => new { Month = a.Month, Amount = a.Amount }).ToList();
            return Json(formattedData);
        }

        public async Task<IActionResult> GetPendingOTCountForSupervisor(string userId)
        {
            try
            {
                var count = await _personalActionService.GetPendingActionsCountForSupervisor(ActionType.Horas_Extra, userId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener la cantidad de horas extra pendientes de aprobar: {ex.Message}");
            }
        }
    }
}
