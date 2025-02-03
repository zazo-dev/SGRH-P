using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGRH.Web.Enums;
using SGRH.Web.Models;
using SGRH.Web.Models.Data;
using SGRH.Web.Models.Entities;
using SGRH.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SGRH.Web.Controllers
{
    [Authorize]

    public class WarningController : Controller
    {
        private readonly IPersonalActionService _personalActionService;
        private readonly IWarningService _warningService;

        public WarningController(SgrhContext context, IWarningService warningService, IPersonalActionService personalActionService)
        {
            _warningService = warningService;
            _personalActionService = personalActionService;
        }

        // GET: /Warning/Index
        [Authorize(Roles = "Administrador, SupervisorRH, SupervisorDpto")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Titulo = "Gestión de Amonestaciones";
            ViewBag.NombreUbicacion = "Listado de Amonestaciones";
            var currentUserWarnings = await _warningService.GetWarnings(User);
            ViewBag.Notifications = await GetLatestNotifications();
            return View(currentUserWarnings);

        }
        private async Task<List<WarningViewModel>> GetLatestNotifications()
        {
            // Obtener las últimas Amonestaciones del usuario actual
            var currentUserWarnings = await _warningService.GetLatestWarnings(User);

            // Convertir las Amonestaciones en vista de modelo
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

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var warning = await _warningService.GetWarningById(id.Value);
            if (warning == null)
            {
                return NotFound();
            }

            var warningViewModel = new WarningViewModel
            {
                Id_Warnings = warning.Id_Warnings,
                Reason = warning.Reason,
                Observations = warning.Observations
            };

            ViewBag.Notifications = await GetLatestNotifications();

            return View(warningViewModel);
        }



        // GET: /Warning/Create
        public IActionResult Create()
        {
            ViewBag.Titulo = "Gestión de Amonestaciones";
            ViewBag.NombreUbicacion = "Registrar Amonestación";
            return View();
        }

        // POST: /Warning/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WarningViewModel model, string userId)
        {
            if (ModelState.IsValid)
            {
                var supervisorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var personalAction = await _personalActionService.CreatePersonalAction(ActionType.Amonestaciones, model.Reason, supervisorId);

                var (success,message) = await _warningService.CreateWarning(supervisorId, model, personalAction, userId);
                if (success)
                    TempData["SuccessMessage"] = "Amonestación registrada de manera correcta.";
                else if (!string.IsNullOrEmpty(message))
                    TempData["ErrorMessage"] = message;
                else
                    TempData["ErrorMessage"] = "Error inesperado al registrar la amonestación.";
                return RedirectToAction("Index");
            }
            return View(model);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var warning = await _warningService.GetWarningById(id.Value);
            if (warning == null)
            {
                return NotFound();
            }

            var warningViewModel = new WarningViewModel
            {
                Id_Warnings = warning.Id_Warnings,
                Reason = warning.Reason,
                Observations = warning.Observations
            };
            ViewBag.Notifications = await GetLatestNotifications();
            return View(warningViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WarningViewModel model)
        {
            if (id != model.Id_Warnings)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var success = await _warningService.UpdateWarning(id, model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Amonestación actualizada de manera correcta.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error al actualizar la amonestación.");
                }
            }
            return View(model);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var warning = await _warningService.GetWarningById(id.Value);
            if (warning == null)
            {
                return NotFound();
            }

            var warningViewModel = new WarningViewModel
            {
                // Asignar propiedades del objeto Warning al WarningViewModel
                Id_Warnings = warning.Id_Warnings,
                Reason = warning.Reason,
                Observations = warning.Observations
            };
            ViewBag.Notifications = await GetLatestNotifications();

            return View(warningViewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _warningService.DeleteWarning(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Amonestación eliminada de manera correcta.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error al eliminar la amonestación.";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<JsonResult> GetWarnings(string userId)
        {
            var warningsCount = await _warningService.GetWarningByUser(userId);

            return Json(warningsCount);
        }

        [HttpGet]
        public async Task<JsonResult> GetWarningsBySupervisor(string userId)
        {
            var warnings = await _personalActionService.GetWarningsBySupervisor(ActionType.Amonestaciones, userId);

            return Json(warnings);

        }

        public async Task<JsonResult> GetWarningsCount()
        {
            var warningsCount = await _warningService.GetWarningsCount();

            return Json(warningsCount);
        }

        public async Task<IActionResult> MyWarnings()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Usuario no identificado, debe iniciar sesión para ver sus amonestaciones.";
                return RedirectToAction("Login", "Account");
            }

            var myWarnings = await _warningService.GetMyWarnings(userId);
            return View(myWarnings);
        }
    }
}
