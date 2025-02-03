using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SGRH.Web.Migrations;
using SGRH.Web.Models.Entities;
using SGRH.Web.Models.ViewModels;
using SGRH.Web.Services;
using System.Security.Claims;

namespace SGRH.Web.Controllers
{
    [Authorize(Roles = "SupervisorRH, Administrador")]
    public class LayoffsController : Controller
    {
        private readonly IPersonalActionService _personalActionService;
        private readonly ILayoffsService _layoffsService;
        private readonly UserManager<User> _userManager;

        public LayoffsController(IPersonalActionService personalActionService, ILayoffsService layoffsService, UserManager<User> userManager)
        {
            _personalActionService = personalActionService;
            _layoffsService = layoffsService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Titulo = "Gestión de Despidos";
            ViewBag.NombreUbicacion = "Lista de despidos";
            var layoffs = await _layoffsService.GetLayoffs();
            return View(layoffs);
        }

        [Authorize]
        public async Task<IActionResult>CreateLayoff()
        {
            ViewBag.Titulo = "Gestión de Despidos";
            ViewBag.NombreUbicacion = "Registrar Despido";

            // Obtener el usuario actualmente autenticado
            var user = await _userManager.GetUserAsync(User);

            var model = new CreateLayoffViewModel
            {
                DismissalDate = DateTime.Today,
                RegisteredBy = user.FullName
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLayoff(CreateLayoffViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                model.currentUserId = currentUserId;

                var (success, message) = await _layoffsService.CreateLayoff(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Despido registrado de manera exitosa.";
                    return RedirectToAction("Index");
                }
                else if (!string.IsNullOrEmpty(message))
                {
                    TempData["ErrorMessage"] = message;
                }
                else
                {
                    TempData["ErrorMessage"] = "Error inesperado al registrar la nómina del empleado.";
                }
            }
            
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> EditLayoff(int layoffId)
        {
            ViewBag.Titulo = "Gestión de Despidos";
            ViewBag.NombreUbicacion = "Editar Despido";

            var layoff = await _layoffsService.GetLayoffById(layoffId);
            if (layoff == null)
            {
                TempData["ErrorMessage"] = "No se encontró el despido.";
                return RedirectToAction("Index");
            }

            var user = layoff.PersonalAction.User;

            if(user == null)
            {
                TempData["ErrorMessage"] = "No se encontró el usuario.";
                return RedirectToAction("Index");
            }


            var model = new UpdateLayoffViewModel
            {
                LayoffId = layoffId,
                HasEmployerResponsibility = layoff.HasEmployerResponsibility,
                DismissalDate = layoff.DismissalDate,
                DismissalCause = layoff.DismissalCause,
                RegisteredBy = layoff.RegisteredBy
            };

            if (layoff.PersonalAction != null && layoff.PersonalAction.User != null)
            {
                model.userId = layoff.PersonalAction.User.Id;
                model.PersonalAction = layoff.PersonalAction;
            }


            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditLayoff(int layoffId, UpdateLayoffViewModel model)
        {
            if (ModelState.IsValid)
            {
                var (success, message) = await _layoffsService.EditLayoff(layoffId, model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Despido actualizado de manera exitosa.";
                    return RedirectToAction("Index");
                }
                else if (!string.IsNullOrEmpty(message))
                {
                    TempData["ErrorMessage"] = message;
                }
                else
                {
                    TempData["ErrorMessage"] = "Error inesperado al actualizar el despido.";
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLayoff(int layoffId)
        {
            var (success, message) = await _layoffsService.DeleteLayoff(layoffId);

            if (success)
            {
                TempData["SuccessMessage"] = "Despido eliminado de manera exitosa.";
                return RedirectToAction("Index");
            }
            else if (!string.IsNullOrEmpty(message))
            {
                TempData["ErrorMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = "Error inesperado al eliminar el despido.";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> LayoffDetails(int layoffId) 
        {
            ViewBag.Titulo = "Gestión de Despidos";
            ViewBag.NombreUbicacion = "Detalles Despido";

            var layoff = await _layoffsService.GetLayoffById(layoffId);
            if (layoff == null)
            {
                TempData["ErrorMessage"] = "No se encontró el despido.";
                return RedirectToAction("Index");
            }

            var user = layoff.PersonalAction.User;

            if (user == null)
            {
                TempData["ErrorMessage"] = "No se encontró el usuario.";
                return RedirectToAction("Index");
            }


            var model = new DetailsLayoffViewModel
            {
                LayoffId = layoffId,
                HasEmployerResponsibility = layoff.HasEmployerResponsibility,
                HasProcessed = layoff.HasProcessed,
                DismissalDate = layoff.DismissalDate,
                DismissalCause = layoff.DismissalCause,
                RegisteredBy = layoff.RegisteredBy
            };

            // Cargar la información del usuario a través de PersonalAction
            if (layoff.PersonalAction != null && layoff.PersonalAction.User != null)
            {
                model.userId = layoff.PersonalAction.User.Id;
                model.PersonalAction = layoff.PersonalAction;
            }


            return View(model);
        }

        public async Task<JsonResult> CountLayoffs()
        {
            var layoffs = await _layoffsService.GetLayoffsCount();
            return Json(layoffs);
        }
    }
}
