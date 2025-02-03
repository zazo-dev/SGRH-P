using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SGRH.Web.Enums;
using SGRH.Web.Models;
using SGRH.Web.Models.Entities;
using SGRH.Web.Models.ViewModels;
using SGRH.Web.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SGRH.Web.Controllers
{
    [Authorize]
    public class VacationsController : Controller
    {
        private readonly IPersonalActionService _personalActionService;
        private readonly IVacationService _vacationService;
        private readonly UserManager<User> _userManager;
        private readonly IWarningService _warningService;

        public VacationsController(IPersonalActionService personalActionService, IVacationService vacationService, UserManager<User> userManager, IWarningService warningService)
        {
            _personalActionService = personalActionService;
            _vacationService = vacationService;
            _userManager = userManager;
            _warningService = warningService;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            ViewBag.Titulo = "Gestión de Vacaciones";
            ViewBag.NombreUbicacion = "Lista de Solicitudes";
            ViewBag.Notifications = await GetLatestNotifications();
            var vacations = await _vacationService.GetVacations();
            return View(vacations);
        }

        public async Task<IActionResult> MyVacationsRequests()
        {
            ViewBag.Titulo = "Gestión de Vacaciones";
            ViewBag.NombreUbicacion = "Lista de mis solicitudes de vacaciones";
            ViewBag.Notifications = await GetLatestNotifications();
            var vacations = await _vacationService.GetVacationsUser(User);
            return View(vacations);
        }


        [Authorize]
        public async Task<IActionResult> RequestVacation()
        {
            ViewBag.Titulo = "Gestión de Vacaciones";
            ViewBag.NombreUbicacion = "Solicitud de Vacaciones";
            ViewBag.Notifications = await GetLatestNotifications();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RequestVacation(VacationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    return RedirectToAction("MyVacationsRequests");
                }

                var personalAction = await _personalActionService.CreatePersonalAction(ActionType.Vacaciones, model.Description, userId);

                var vacation = new Vacation
                {
                    PersonalAction = personalAction,
                    Start_Date = model.StartDate,
                    End_Date = model.EndDate,
                    Comments = model.Comments
                };

                var (success, message) = await _vacationService.CreateVacation(personalAction, vacation.Start_Date, vacation.End_Date, vacation.RequestedDays, vacation.Comments);
                if (success)
                    TempData["SuccessMessage"] = "Se registró la solicitud exitosamente";
                else if (!string.IsNullOrEmpty(message))
                    TempData["ErrorMessage"] = message;
                else
                    TempData["ErrorMessage"] = "Error al solicitar su solicitud de vacaciones.";

                return RedirectToAction("MyVacationsRequests");
            }
            return View(model);
        }
        [Authorize(Roles = "Administrador, SupervisorRH, SupervisorDpto")]
        public async Task<IActionResult> VacationsManagement()
        {
            ViewBag.Titulo = "Gestión de Vacaciones";
            ViewBag.NombreUbicacion = "Gestión de vacaciones";
            ViewBag.Notifications = await GetLatestNotifications();
            var pendingVacations = await _vacationService.VacationsRequests(Status.Pendiente, User);

            return View(pendingVacations);
        }
        [Authorize(Roles = "Administrador, SupervisorRH, SupervisorDpto")]
        [HttpPost]
        public async Task<IActionResult> Approve(int vacationId)
        {
            var (result, message) = await _vacationService.ApproveVacationRequest(vacationId, User);

            if (result)
            {
                TempData["SuccessMessage"] = "Solicitud aprobada exitosamente.";
                RedirectToAction("VacationsManagement");
            }
            else if (!string.IsNullOrEmpty(message))
            {
                TempData["ErrorMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Error inesperado al aprobar la solicitud de vacaciones.";
                return RedirectToAction("VacationsManagement");

            }

            return RedirectToAction("VacationsManagement");

        }
        [Authorize(Roles = "Administrador, SupervisorRH, SupervisorDpto")]
        [HttpPost]
        public async Task<IActionResult> Reject(int vacationId)
        {
            var (result, message) = await _vacationService.RejectVacationRequest(vacationId, User);

            if (result)
                TempData["SuccessMessage"] = "Solicitud rechazada exitosamente.";
            else if (!string.IsNullOrEmpty(message))
                TempData["ErrorMessage"] = message;
            else
                TempData["ErrorMessage"] = "Error inesperado al rechazar la solicitud de vacaciones.";
            return RedirectToAction("VacationsManagement");

        }

        public async Task<IActionResult> VacationsBalance()
        {
            ViewBag.Titulo = "Gestión de Vacaciones";
            ViewBag.NombreUbicacion = "Consulta del saldo de vacaciones";
            ViewBag.Notifications = await GetLatestNotifications();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Usuario no encontrado, favor validar.";
                return View();
            }

            var availableDays = await _vacationService.VacationBalance(userId);

            return View(availableDays);

        }

        public async Task<IActionResult> VacationsBalanceAJAX()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Usuario no encontrado, favor validar." });
            }

            var availableDays = await _vacationService.VacationBalance(userId);

            return Json(new { success = true, balance = availableDays });
        }

        [Authorize(Roles = "SupervisorRH, Administrador")]
        public async Task<IActionResult> AddVacations()
        {
            ViewBag.Titulo = "Gestión de Vacaciones";
            ViewBag.NombreUbicacion = "Añadir saldo actual de vacaciones";
            ViewBag.Notifications = await GetLatestNotifications();
            return View();
        }

        [Authorize(Roles = "SupervisorRH, Administrador")]
        [HttpPost]
        public async Task<IActionResult> AddInitialVacationsDays(string userId, int days)
        {
            var result = await _vacationService.AddInitialVacationDays(userId, days);

            if (result.success)
            {
                TempData["SuccessMessage"] = "Días agregados de manera exitosa.";
            }
            else
            {
                TempData["ErrorMessage"] = result.errorMessage ?? "Error al agregar el saldo actual de vacaciones.";
            }

            return RedirectToAction("AddVacations");
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

        public async Task<JsonResult> GetBalance(string userId)
        {
            var availableDays = await _vacationService.VacationBalance(userId);

            return Json(availableDays);
        }


        public async Task<JsonResult> GetVacationsDayTaken(string userId)
        {
            var vacationDaysTaken = await _vacationService.VacationDaysTaken(userId);

            return Json(vacationDaysTaken);
        }

        public async Task<IActionResult> GetPendingVacationsCountForSupervisor(string userId)
        {
            try
            {
                var count = await _personalActionService.GetPendingActionsCountForSupervisor(ActionType.Vacaciones, userId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener la cantidad de vacaciones pendientes: {ex.Message}");
            }
        }

    }
}
