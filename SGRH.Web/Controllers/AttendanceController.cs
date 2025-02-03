using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SGRH.Web.Models;
using SGRH.Web.Services;
using System.Security.Claims;

namespace SGRH.Web.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IAbsenceService _absenceService;
        private readonly IWarningService _warningService;


        public AttendanceController(IAttendanceService attendanceService, IAbsenceService absenceService, IWarningService warningService)
        {
            _attendanceService = attendanceService;
            _absenceService = absenceService;
            _warningService = warningService;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            ViewBag.Titulo = "Gestión de Asistencia";
            ViewBag.NombreUbicacion = "Ver Asistencias";
            var attendances = await _attendanceService.GetAttendances(User);
            ViewBag.Notifications = await GetLatestNotifications();
            return View(attendances);
        }

        public async Task<IActionResult> MyAttendances(string userId)
        {
            var myAttendance = await _attendanceService.GetMyAttendance(userId);
            return View(myAttendance);
        }

        public async Task<IActionResult> MyAbsences(string userId)
        {
            var myAbsences = await _absenceService.GetMyAbsences(userId);
            return View(myAbsences);
        }

        public IActionResult RegisterAttendance()
        {
            ViewBag.Titulo = "Gestión de Asistencia";
            ViewBag.NombreUbicacion = "Registrar Asistencia";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAttendancePost()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Usuario no identificado, debe iniciar sesion para registrar su hora de entrada.";
                return RedirectToAction("RegisterAttendance");
            }

            bool hasEntryForToday = await _attendanceService.HasEntryForToday(userId);

            if(hasEntryForToday)
            {
                TempData["ErrorMessage"] = "Ya ha registrado su entrada para hoy.";
                return RedirectToAction("Index");
            }

            var success = await _attendanceService.RegisterEntry(userId);

            if (success)
                TempData["SuccessMessage"] = "Entrada registrada correctamente.";
            else
                TempData["ErrorMessage"] = "Error al registrar la entrada.";

            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterExitPost()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Usuario no identificado, debe iniciar sesion para registrar su hora de salida.";
                return RedirectToAction("RegisterAttendance");
            }
            
            var sucess = await _attendanceService.RegisterExit(userId);
            if (!sucess)
            {
                TempData["ErrorMessage"] = "Error al registrar la salida.";
            }

            TempData["SuccessMessage"] = "Salida registrada correctamente.";
            return RedirectToAction("Index");
        }

        
        public async Task<IActionResult> RegisterAbsence()
        {
            ViewBag.Titulo = "Gestión de Asistencia";
            ViewBag.NombreUbicacion = "Registrar Ausencia";
            var categories = await _absenceService.GetAbsenceCategories();
            ViewBag.Categories = categories.Select(c=>new SelectListItem { Value = c.Id_Absence_Category.ToString(), Text = c.Category_Name }).ToList();
            ViewBag.Notifications = await GetLatestNotifications();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAbsencePost(AbsenceViewModel absenceModel)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Usuario no identificado, debe iniciar sesion para registrar una ausencia.";
                return RedirectToAction("ViewAbsences");
            }

            var documentContents = new List<byte[]>();
            var documentFileNames = new List<string>();
            var documentContentTypes = new List<string>();

            if (absenceModel.Documentation != null)
            {
                foreach (var file in absenceModel.Documentation)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        documentContents.Add(memoryStream.ToArray());
                        documentFileNames.Add(file.FileName);
                        documentContentTypes.Add(file.ContentType);
                    }
                }
            }

            var success = await _absenceService.RegisterAbsence(userId, absenceModel, documentContents, documentFileNames, documentContentTypes);

            if (success)
                TempData["SuccessMessage"] = "Ausencia registrada correctamente.";
            else
                TempData["ErrorMessage"] = "Error al registrar la ausencia.";

            return RedirectToAction("ViewAbsences");
        }

        [Authorize]
        public async Task<IActionResult> ViewAbsences()
        {
            ViewBag.Titulo = "Gestión de Asistencia";
            ViewBag.NombreUbicacion = "Ver Ausencias";
            var view = await _absenceService.GetAbsences(User);
            ViewBag.Notifications = await GetLatestNotifications();
            return View(view);
        }

        [HttpGet]
        public async Task<IActionResult> EditAbsence(int id)
        {
            ViewBag.Titulo = "Gestión de Asistencia";
            ViewBag.NombreUbicacion = "Editar Ausencia";
            var categories = await _absenceService.GetAbsenceCategories();
            ViewBag.Categories = categories.Select(c => new SelectListItem { Value = c.Id_Absence_Category.ToString(), Text = c.Category_Name }).ToList();
            ViewBag.Notifications = await GetLatestNotifications();

            var absence = await _absenceService.GetAbsenceById(id);
            if (absence == null)
            {
                return NotFound();
            }

            var model = new AbsenceViewModel
            {
                Id = absence.AbsenceId,
                Category = absence.AbsenceCategory.Id_Absence_Category.ToString(),
                StartDate = absence.Start_Date,
                EndDate = absence.End_Date,
                Comments = absence.Absence_Comments,
                Documentations = absence.Document == null ? new List<DocumentViewModel>() : absence.Document.Select(d => new DocumentViewModel
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    ContentType = d.ContentType,
                    Content = d.Content
                }).ToList()
            };

            // Pasar el modelo a la vista
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditAbsencePost(int id, AbsenceViewModel absenceModel)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Usuario no identificado, debe iniciar sesion para editar una ausencia.";
                return RedirectToAction("ViewAbsences");
            }

            var documentContents = new List<byte[]>();
            var documentFileNames = new List<string>();
            var documentContentTypes = new List<string>();

            if (absenceModel.Documentation != null)
            {
                foreach (var formFile in absenceModel.Documentation)
                {
                    using var memoryStream = new MemoryStream();
                    await formFile.CopyToAsync(memoryStream);
                    documentContents.Add(memoryStream.ToArray());
                    documentFileNames.Add(formFile.FileName);
                    documentContentTypes.Add(formFile.ContentType);
                }
            }

            var success = await _absenceService.UpdateAbsence(userId, id, absenceModel, documentContents, documentFileNames, documentContentTypes);

            if (success)
                TempData["SuccessMessage"] = "Ausencia actualizada correctamente.";
            else
                TempData["ErrorMessage"] = "Error al actualizar la ausencia.";

            return RedirectToAction("ViewAbsences");
        }

        [Authorize(Roles = "Administrador, SupervisorRH, SupervisorDpto")]
        [HttpPost]
        public async Task<IActionResult> DeleteAbsencePost(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Usuario no identificado, debe iniciar sesión para eliminar una ausencia.";
                return RedirectToAction("ViewAbsences");
            }

            var success = await _absenceService.DeleteAbsence(User, id);

            if (success)
                TempData["SuccessMessage"] = "Ausencia eliminada correctamente.";
            else
                TempData["ErrorMessage"] = "Error al eliminar la ausencia.";

            return RedirectToAction("ViewAbsences");
        }

        public async Task<IActionResult> AbsenceDetails(int id)
        {
            ViewBag.Titulo = "Gestión de Asistencia";
            ViewBag.NombreUbicacion = "Ver Detalle de Ausencia";
            ViewBag.Notifications = await GetLatestNotifications();
            var model = await _absenceService.GetAbsenceDetails(id);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        public async Task<IActionResult> DownloadDocument(int documentId)
        {
            var documentContent = await _absenceService.DownloadDocument(documentId);
            if (documentContent == null)
            {
                return NotFound();
            }

            return File(documentContent.Content, documentContent.ContentType, documentContent.FileName);

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

        [HttpGet]
        public async Task<IActionResult> GetAttendances(string userId)
        {
            var attendances = await _attendanceService.GetAttendancesByUser(userId);
            return Json(attendances);
        }


        [HttpGet]
        public async Task<IActionResult> GetAbsences(string userId)
        {
            var absences = await _absenceService.GetAbsencesByUser(userId);
            return Json(absences);
        }

    }
}
