using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGRH.Web.Models.Entities;
using SGRH.Web.Services;

namespace SGRH.Web.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Titulo = "Gestión de Departamentos";
            ViewBag.NombreUbicacion = "Departamentos";
            var departments = await _departmentService.GetDepartments();
            return View(departments);
        }

        public async Task<JsonResult> CountDepartments()
        {
            var departments = await _departmentService.GetDepartmentCount();
            return Json(departments);
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult CreateDepartment()
        {
            ViewBag.Titulo = "Gestión de Departamentos";
            ViewBag.NombreUbicacion = "Registrar Departamento";

            var model = new Department();

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CreateDepartment(Department model)
        {
            if (ModelState.IsValid)
            {
                var (success, errorMessage) = await _departmentService.CreateDepartment(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Departamento registrado correctamente.";
                    return RedirectToAction("Index");
                }
                else if (!string.IsNullOrEmpty(errorMessage))
                {
                    TempData["ErrorMessage"] = errorMessage;
                }
                else
                {
                    TempData["ErrorMessage"] = "Error inesperado al registrar el departamento.";
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
        public async Task<IActionResult> UpdateDepartment(int id)
        {
            ViewBag.Titulo = "Gestión de Departamentos";
            ViewBag.NombreUbicacion = "Actualizar Departamento";

            var department = await _departmentService.GetDepartmentById(id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> UpdateDepartment(Department model)
        {
            if (ModelState.IsValid)
            {
                var (success, errorMessage) = await _departmentService.UpdateDepartment(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Departamento actualizado correctamente.";
                    return RedirectToAction("Index");
                }
                else if (!string.IsNullOrEmpty(errorMessage))
                {
                    TempData["ErrorMessage"] = errorMessage;
                }
                else
                {
                    TempData["ErrorMessage"] = "Error inesperado al actualizar el departamento.";
                }
            }

            TempData["ErrorMessage"] = "No es posible procesar el formulario sin los campos requeridos.";

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var (success, errorMessage) = await _departmentService.DeleteDepartment(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Departamento eliminado correctamente.";
                return RedirectToAction("Index");
            }
            else if (!string.IsNullOrEmpty(errorMessage))
            {
                TempData["ErrorMessage"] = errorMessage;
            }
            else
            {
                TempData["ErrorMessage"] = "Error inesperado al eliminar el departamento.";
            }

            return RedirectToAction("Index");
        }


    }
}
