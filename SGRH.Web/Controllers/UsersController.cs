using Microsoft.AspNetCore.Mvc;
using SGRH.Web.Models;
using SGRH.Web.Models.Data;
using SGRH.Web.Services;
using SGRH.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using SGRH.Web.Enums;
using System.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Authorization;

namespace SGRH.Web.Controllers
{
    
    public class UsersController : Controller
    {
        private readonly SgrhContext _context;
        private readonly IServiceUser _serviceUser;
        private readonly IWarningService _warningService;

        public UsersController(SgrhContext context, IServiceUser serviceUser, IWarningService warningService)
        {
            _context = context;
            _serviceUser = serviceUser;
            _warningService = warningService;
        }

        [Authorize(Roles = "Administrador, SupervisorRH")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Titulo = "Gestión de Usuarios";
            ViewBag.NombreUbicacion = "Ver Usuarios";
            ViewBag.Notifications = await GetLatestNotifications();
            var users = await _serviceUser.GetAllUsersWithDetails();

            return View(users);
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

       [Authorize(Roles = "Administrador, SupervisorRH")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Titulo = "Gestión de Usuarios";
            ViewBag.NombreUbicacion = "Crear Usuario";

            var departments = await _serviceUser.GetDepartments();
            var positions = await _serviceUser.GetPositions();
            var workPeriods = await _serviceUser.GetWorkPeriods();

            // Inicializar la lista de roles aquí
            var roles = Enum.GetValues(typeof(UserType))
                .Cast<UserType>()
                .Select(r => new SelectListItem
                {
                    Value = ((int)r).ToString(),
                    Text = r.ToString()
                })
                .ToList();

            UserViewModel model = new UserViewModel
            {
                Departments = departments.Select(d => new SelectListItem { Value = d.Id_Department.ToString(), Text = d.Department_Name }),
                Positions = positions.Select(p => new SelectListItem { Value = p.Id_Position.ToString(), Text = p.Position_Name }),
                WorkPeriods = workPeriods.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.PeriodName }),
                Roles = roles,
            };
            ViewBag.Notifications = await GetLatestNotifications();

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {

            if (ModelState.IsValid)
            {
                var (success, msg) = await _serviceUser.CreateUser(model, HttpContext);

                if (success)
                {
                    TempData["SuccessMessage"] = msg;
                    return RedirectToAction("Index", "Users");
                }
                else if (!string.IsNullOrEmpty(msg))
                {
                    TempData["ErrorMessage"] = msg;
                    return View(model);
                }
            }

            ViewBag.Titulo = "Gestión de Usuarios";
            ViewBag.NombreUbicacion = "Crear Usuario";

            var departments = await _serviceUser.GetDepartments();
            var positions = await _serviceUser.GetPositions();
            var workPeriods = await _serviceUser.GetWorkPeriods();

            // Inicializar la lista de roles aquí
            var roles = Enum.GetValues(typeof(UserType))
                .Cast<UserType>()
                .Select(r => new SelectListItem
                {
                    Value = ((int)r).ToString(),
                    Text = r.ToString()
                })
                .ToList();

            model.Departments = departments.Select(d => new SelectListItem { Value = d.Id_Department.ToString(), Text = d.Department_Name });
            model.Positions = positions.Select(p => new SelectListItem { Value = p.Id_Position.ToString(), Text = p.Position_Name });
            model.WorkPeriods = workPeriods.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.PeriodName });
            model.Roles = roles;
            
            ViewBag.Notifications = await GetLatestNotifications();


            return View(model);
        }

        [HttpGet]
       [Authorize(Roles = "Administrador, SupervisorRH")]
        public async Task<IActionResult> Edit(string id)
        {
            ViewBag.Titulo = "Gestión de Usuarios";
            ViewBag.NombreUbicacion = "Editar Usuario";
            ViewBag.Notifications = await GetLatestNotifications();

            var userViewModel = await _serviceUser.GetUserViewModelById(id);

            var roles = Enum.GetValues(typeof(UserType))
                .Cast<UserType>()
                .Select(r => new SelectListItem
                {
                    Value = ((int)r).ToString(),
                    Text = r.ToString()
                })
                .ToList();

            if (userViewModel != null)
            {
                var departments = await _serviceUser.GetDepartments();
                var positions = await _serviceUser.GetPositions();
                var workPeriods = await _serviceUser.GetWorkPeriods();

                userViewModel.Departments = departments.Select(d => new SelectListItem { Value = d.Id_Department.ToString(), Text = d.Department_Name });
                userViewModel.Positions = positions.Select(p => new SelectListItem { Value = p.Id_Position.ToString(), Text = p.Position_Name });
                userViewModel.WorkPeriods = workPeriods.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.PeriodName });
                userViewModel.Roles = roles;
                return View(userViewModel);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            var result = await _serviceUser.UpdateUser(model);

            if (result)
            {
                TempData["SuccessMessage"] = "Usuario editado de manera exitosa.";
                return RedirectToAction("Index", "Users");
            }

            TempData["ErrorMessage"] = "Ocurrió un error al editar el usuario. Por favor, intenta de nuevo.";

            return RedirectToAction("Index");
        }

        [HttpGet]
          [Authorize(Roles = "Administrador, SupervisorRH")]
        public async Task<IActionResult> Details(string id)
        {
            ViewBag.Titulo = "Gestión de Usuarios";
            ViewBag.NombreUbicacion = "Detalles del Usuario";

            var userViewModel = await _serviceUser.GetUserViewModelById(id);

            if (userViewModel != null)
            {
                return View(userViewModel);
            }
            ViewBag.Notifications = await GetLatestNotifications();
            return NotFound();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador, SupervisorRH")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var userViewModel = await _serviceUser.GetUserViewModelById(id);

                if (userViewModel == null)
                {
                    TempData["ErrorMessage"] = "El usuario no pudo ser encontrado.";
                    return RedirectToAction("Index");
                }


                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var (success, errorMessage) = await _serviceUser.DeleteUser(userViewModel, currentUserId);

                if (success)
                    TempData["SuccessMessage"] = "Usuario eliminado de manera exitosa.";
                else if (!string.IsNullOrEmpty(errorMessage))
                    TempData["ErrorMessage"] = errorMessage;
                else
                    TempData["ErrorMessage"] = "Ocurrio un error al eliminar el usuario.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrio un error al eliminar el usuario. Por favor, intenta de nuevo.";
                return RedirectToAction("Index");
            }
        }

        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            ViewBag.Titulo = "Mi Perfil";
            ViewBag.NombreUbicacion = "Editar Perfil";
            ViewBag.Notifications = await GetLatestNotifications();

            var userProfile = await _serviceUser.GetCurrentUserProfile();

            if(userProfile != null)
            {
                return View("Profile", userProfile);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Profile(UserViewModel model)
        {
            var result = await _serviceUser.UpdateUser(model);

            if (result)
            {
                TempData["SuccessMessage"] = "Perfil editado de manera exitosa.";
                return RedirectToAction("Index", "Home");
            }

            TempData["ErrorMessage"] = "Ocurrio un error al editar el perfil. Por favor, intenta de nuevo.";
            
            var departments = await _serviceUser.GetDepartments();
            var positions = await _serviceUser.GetPositions();

            model.Departments = departments.Select(d => new SelectListItem { Value = d.Id_Department.ToString(), Text = d.Department_Name });
            model.Positions = positions.Select(p => new SelectListItem { Value = p.Id_Position.ToString(), Text = p.Position_Name });
            ViewBag.Notifications = await GetLatestNotifications();
            return View("Edit", model);

        }

        [HttpGet]
        public async Task<IActionResult> GetPositionsByDepartment(int departmentId)
        {
            var positions = await _serviceUser.GetPositionsByDepartment(departmentId);

            var result = positions.Select(p => new SelectListItem { Value = p.Id_Position.ToString(), Text = p.Position_Name }).ToList();

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserData(string userDni)
        {
            try
            {
                var userData = await _serviceUser.GetUserData(userDni);
                return Content(userData, "application/json");
            }
            catch (Exception)
            {
                return NotFound("Usuario no encontrado.");
            }
        }

        public async Task<JsonResult> GetEmployeeCountByDepartment(string userId)
        {
            var Count = await _serviceUser.GetEmployeeCountByDepartment(userId);

            return Json(Count);
        }

        public async Task<JsonResult> GetEmployeesCount()
        {
            var Count = await _serviceUser.GetEmployeeCount();

            return Json(Count);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUserStatus(string id, bool status)
        {
            var result = await _serviceUser.SetStatus(id, status);

            if (result)
            {
                return Json(new { success = true, message = "Estado de usuario actualizado." });
            }
            else
            {
                return Json(new { success = false, message = "Ocurrió un error al activar el usuario. Por favor, intenta de nuevo." });
            }
        }

    }
}
