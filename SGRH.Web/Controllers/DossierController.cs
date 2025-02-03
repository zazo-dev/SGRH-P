using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGRH.Web.Models;
using SGRH.Web.Models.Data;
using SGRH.Web.Models.Entities;
using SGRH.Web.Services;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace SGRH.Web.Controllers
{
    [Authorize]
    public class DossiersController : Controller
    {
        private readonly SgrhContext _context;
        private readonly IDossierService _dossierService;
        private readonly IServiceUser _serviceUser;
        private readonly IAbsenceService _absenceService;
        private readonly IWarningService _warningService;


        public DossiersController(SgrhContext context, IDossierService dossierService, IServiceUser serviceUser, IAbsenceService absenceService, IWarningService warningService)
        {
            _context = context;
            _dossierService = dossierService;
            _serviceUser = serviceUser;
            _absenceService = absenceService;
            _warningService = warningService;
        }

        // GET: Dossiers
        [Authorize]
        public async Task<IActionResult> Index()
        {
            ViewBag.Titulo = "Gestión de Expedientes";
            ViewBag.NombreUbicacion = "Ver Expedientes";
            var dossiers = await _dossierService.GetDossiers(User);
            ViewBag.Notifications = await GetLatestNotifications();
            return View(dossiers);
        }

        // GET: Dossiers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            ViewBag.Titulo = "Gestión de Expedientes";
            ViewBag.NombreUbicacion = "Detalles de Expediente";

            if (id == null)
            {
                return NotFound();
            }

            var dossierDetails = await _dossierService.GetDossierDetails(id.Value);
            if (dossierDetails == null)
            {
                return NotFound();
            }

            return View(dossierDetails);
        }

        [Authorize(Roles = "SupervisorRH , Administrador")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Titulo = "Gestión de Expedientes";
            ViewBag.NombreUbicacion = "Registrar Expediente";
            ViewBag.Notifications = await GetLatestNotifications();
            return View();
        }

        // POST: Dossiers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SupervisorRH , Administrador")]
        public async Task<IActionResult> Create(DossierViewModel model)
        {
            var documentContents = new List<byte[]>();
            var documentFileNames = new List<string>();
            var documentContentTypes = new List<string>();

            if (model.Documentation != null)
            {
                foreach (var file in model.Documentation)
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


            if (ModelState.IsValid)
            {
                var supervisorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                model.Date = DateTime.Now;
                var success = await _dossierService.CreateDossier(supervisorId, model, documentContents, documentFileNames, documentContentTypes);
                if (success)
                    TempData["SuccessMessage"] = "Expediente registrado correctamente.";
                else
                    TempData["ErrorMessage"] = "Error al registrar el expediente.";

            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int? id)
        {
            ViewBag.Titulo = "Gestión de Expedientes";
            ViewBag.NombreUbicacion = "Editar Expediente";

            if (id == null)
            {
                return NotFound();
            }

            var dossier = await _context.Dossiers
                .Include(d => d.Document)
                .FirstOrDefaultAsync(m => m.Id_Record == id);

            if (dossier == null)
            {
                return NotFound();
            }

            var model = new DossierViewModel
            {
                // Asigna los valores del dossier al viewModel
                DocumentType = dossier.DocumentType,
                Description = dossier.Description,
                Date = dossier.Date ?? DateTime.Now,
                Documentations = dossier.Document == null ? new List<DossierDocumentViewModel>() : dossier.Document.Select(d => new DossierDocumentViewModel
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    ContentType = d.ContentType,
                    Content = d.Content
                }).ToList()
            };

            return View(model);
        }



        // POST: Dossiers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SupervisorRH , Administrador")]
        public async Task<IActionResult> Edit(int id, DossierViewModel dossierViewModel)
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

            if (dossierViewModel.Documentation != null)
            {
                foreach (var formFile in dossierViewModel.Documentation)
                {
                    using var memoryStream = new MemoryStream();
                    await formFile.CopyToAsync(memoryStream);
                    documentContents.Add(memoryStream.ToArray());
                    documentFileNames.Add(formFile.FileName);
                    documentContentTypes.Add(formFile.ContentType);
                }
            }

            var (success, errorMessage) = await _dossierService.UpdateDossier(id, dossierViewModel, documentContents, documentFileNames, documentContentTypes);

            if (success)
                TempData["SuccessMessage"] = "Expediente actualizado correctamente.";
            else if (!string.IsNullOrEmpty(errorMessage))
                TempData["ErrorMessage"] = errorMessage;
            else
                TempData["ErrorMessage"] = "Error al actualizar el expediente.";

            return RedirectToAction("Index");
        }



        // GET: Dossiers/Delete/5
        [Authorize(Roles = "SupervisorRH , Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            ViewBag.Titulo = "Gestión de Expedientes";
            ViewBag.NombreUbicacion = "Eliminar Expediente";

            if (id == null)
            {
                return NotFound();
            }

            var dossier = await _context.Dossiers
                .Include(d => d.Document) // Incluye los documentos relacionados
                .FirstOrDefaultAsync(m => m.Id_Record == id);

            if (dossier == null)
            {
                return NotFound();
            }

            return View(dossier);
        }

        // POST: Dossiers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SupervisorRH , Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dossier = await _context.Dossiers
                .Include(d => d.Document) // Incluye los documentos relacionados
                .FirstOrDefaultAsync(m => m.Id_Record == id);

            if (dossier == null)
            {
                return NotFound();
            }

            var success = await _dossierService.DeleteDossier(id);

            if (success)
                TempData["SuccessMessage"] = "Expediente registrado correctamente.";
            else
                TempData["ErrorMessage"] = "Error al registrar el expediente.";


            return RedirectToAction(nameof(Index));
        }


        private bool DossierExists(int id)
        {
            return _context.Dossiers.Any(e => e.Id_Record == id);
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

        public async Task<JsonResult> GetDocumentsInDossier(string userId)
        {
            var dossierCount = await _dossierService.GetDossierCountByUser(userId);

            return Json(dossierCount);
        }

        public async Task<IActionResult> MyDossier()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Usuario no identificado, debe iniciar sesión para ver su expediente.";
                return RedirectToAction("Login", "Account");
            }

            var myDossier = await _dossierService.GetMyDossier(userId);
            return View(myDossier);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDocument(int documentId)
        {
            var success = await _dossierService.DeleteDocument(documentId);

            if (!success)
            {
                TempData["ErrorMessage"] = "Error al eliminar el documento.";
            }
            else
            {
                TempData["SuccessMessage"] = "Documento eliminado correctamente.";
            }

            // Devolver un objeto JSON indicando el resultado de la operación
            return Json(new { success = success, message = TempData["SuccessMessage"] });
        }

    }
}