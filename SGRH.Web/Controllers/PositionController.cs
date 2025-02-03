using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGRH.Web.Models.Entities;
using SGRH.Web.Services;

namespace SGRH.Web.Controllers
{
    public class PositionController : Controller
    {
        private readonly IPositionService _positionService;
        private readonly IDepartmentService _departmentService;

        public PositionController(IPositionService positionService, IDepartmentService departmentService)
        {
            _positionService = positionService;
            _departmentService = departmentService;
        }

        [Authorize(Roles = "Administrador")]
        public async Task <IActionResult> Index()
        {
            ViewBag.Titulo = "Gestión de Puestos";
            ViewBag.NombreUbicacion = "Puestos";
            var positions = await _positionService.GetPositions();
            return View(positions);
        }

        public async Task<JsonResult> CountPositions()
        {
            var departments = await _positionService.GetPositionsCount();
            return Json(departments);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CreatePosition()
        {
            ViewBag.Titulo = "Gestión de Puestos";
            ViewBag.NombreUbicacion = "Registrar Puesto";

            var departments = await _departmentService.GetDepartments();
            if (departments != null)
            {
                ViewBag.Departments = departments;
            }
            else
            {
                // Si departments es null, asigna una lista vacía o maneja el caso según tus necesidades
                ViewBag.Departments = new List<Department>();
            }


            var model = new Position();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CreatePosition(Position model)
        {
            if (ModelState.IsValid)
            {
                var (success, errorMessage) = await _positionService.CreatePositions(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Puesto registrado correctamente.";
                    return RedirectToAction("Index");
                }
                else if (!string.IsNullOrEmpty(errorMessage))
                {
                    TempData["ErrorMessage"] = errorMessage;
                }
                else
                {
                    TempData["ErrorMessage"] = "Error inesperado al registrar el Puesto.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "No es posible procesar el formulario sin los campos requeridos.";
                return View(model);
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> UpdatePosition(int id)
        {
            ViewBag.Titulo = "Gestión de Puestos";
            ViewBag.NombreUbicacion = "Actualizar Puesto";

            var departments = await _departmentService.GetDepartments();
            if (departments != null)
            {
                ViewBag.Departments = departments;
            }
            else
            {
                // Si departments es null, asigna una lista vacía o maneja el caso según tus necesidades
                ViewBag.Departments = new List<Department>();
            }

            var department = await _positionService.GetPositionById(id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> UpdatePosition(Position model)
        {
            if (ModelState.IsValid)
            {
                var (success, errorMessage) = await _positionService.UpdatePositions(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Puesto actualizado correctamente.";
                    return RedirectToAction("Index");
                }
                else if (!string.IsNullOrEmpty(errorMessage))
                {
                    TempData["ErrorMessage"] = errorMessage;
                }
                else
                {
                    TempData["ErrorMessage"] = "Error inesperado al actualizar el Puesto.";
                }
            }

            TempData["ErrorMessage"] = "No es posible procesar el formulario sin los campos requeridos.";

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePosition(int id)
        {
            var (success, errorMessage) = await _positionService.DeletePosition(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Puesto eliminado correctamente.";
                return RedirectToAction("Index");
            }
            else if (!string.IsNullOrEmpty(errorMessage))
            {
                TempData["ErrorMessage"] = errorMessage;
            }
            else
            {
                TempData["ErrorMessage"] = "Error inesperado al eliminar el Puesto.";
            }

            return RedirectToAction("Index");
        }

    }
}
