using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGRH.Web.Models;
using SGRH.Web.Models.Data;
using SGRH.Web.Models.Entities;
using System.Security.Claims;

namespace SGRH.Web.Services
{
    public class AbsenceService : IAbsenceService
    {
        private readonly SgrhContext _context;
        private readonly UserManager<User> _userManager;

        public AbsenceService(SgrhContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<AbsenceCategory>> GetAbsenceCategories()
        {
            if (_context == null)
            {
                throw new ArgumentNullException(nameof(_context), "El contexto de base de datos no ha sido inicializado.");
            }

            var categories = await _context.AbsenceCategories.ToListAsync();

            if (categories == null)
            {
                throw new InvalidOperationException("La lista de categorías de ausencia es nula después de la consulta a la base de datos.");
            }

            return categories;
        }

        public async Task<IList<Absence>> GetAbsences(ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);
            var currentUser = await _userManager.GetUserAsync(user);
            var userRole = await _userManager.GetRolesAsync(currentUser);

            if (user.IsInRole("Empleado"))
            {
                return await _context.Absences
                    .Where(a => a.User.Id == userId)
                    .Include(a => a.User)
                    .Include(a => a.AbsenceCategory)
                    .ToListAsync();
            }

            if (user.IsInRole("SupervisorDpto"))
            {
                return await _context.Absences
                    .Where(u => u.User.DepartmentId == currentUser.DepartmentId)
                    .Include(a => a.User)
                    .Include(a => a.AbsenceCategory)
                    .ToListAsync();
            }

            return await _context.Absences
                    .Include(a => a.User)
                    .Include(a => a.AbsenceCategory)
                    .ToListAsync();
        }

        public async Task<bool> RegisterAbsence(string userId, AbsenceViewModel model, List<byte[]> documentContents, List<string> documentFileNames, List<string> documentContentTypes)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return false;
                }

                // Convertir el ID de la categoría en un objeto AbsenceCategory
                if (!int.TryParse(model.Category, out int categoryId))
                {
                    return false;
                }

                var absenceCategory = await _context.AbsenceCategories.FindAsync(categoryId);
                if (absenceCategory == null)
                {
                    return false;
                }

                var absence = new Absence
                {
                    User = user,
                    AbsenceCategory = absenceCategory,
                    Start_Date = model.StartDate,
                    End_Date = model.EndDate,
                    Absence_Comments = model.Comments,
                    Document = new List<Document>()
                };

                for (int i = 0; i < documentContents.Count; i++)
                {
                    var document = new Document
                    {
                        FileName = documentFileNames[i],
                        Content = documentContents[i],
                        ContentType = documentContentTypes[i],
                        Size = documentContents[i].Length,
                        CreatedAt = DateTime.Now
                    };

                    absence.Document.Add(document);
                }

                _context.Absences.Add(absence);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateAbsence(string userId, int absenceId, AbsenceViewModel model, List<byte[]> documentContents, List<string> documentFileNames, List<string> documentContentTypes)
        {
            try
            {
                var absence = await _context.Absences.Include(a => a.Document).FirstOrDefaultAsync(a => a.AbsenceId == absenceId);
                if (absence == null)
                {
                    return false;
                }

                if (!int.TryParse(model.Category, out int categoryId))
                {
                    return false;
                }

                var absenceCategory = await _context.AbsenceCategories.FindAsync(categoryId);
                if (absenceCategory == null)
                {
                    return false;
                }

                absence.AbsenceCategory = absenceCategory;
                absence.Start_Date = model.StartDate;
                absence.End_Date = model.EndDate;
                absence.Absence_Comments = model.Comments;

                // Remove existing documents
                _context.Document.RemoveRange(absence.Document);

                // Add new documents
                if (model.Documentation != null)
                {
                    foreach (var formFile in model.Documentation)
                    {
                        using var memoryStream = new MemoryStream();
                        await formFile.CopyToAsync(memoryStream);

                        var document = new Document
                        {
                            FileName = formFile.FileName,
                            Content = memoryStream.ToArray(),
                            ContentType = formFile.ContentType,
                            Size = formFile.Length,
                            CreatedAt = DateTime.Now
                        };

                        absence.Document.Add(document);
                    }
                }

                _context.Absences.Update(absence);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [Authorize(Roles = "SupervisorRh,Empleado,SupervisorDpto")]
        public async Task<bool> DeleteAbsence(ClaimsPrincipal user, int absenceId)
        {
            try
            {
                var userId = _userManager.GetUserId(user);
                var currentUser = await _userManager.GetUserAsync(user);
                var userRole = await _userManager.GetRolesAsync(currentUser);

                if (user == null)
                {
                    return false;
                }

                var absence = await _context.Absences
                    .Include(u=>u.User)
                    .Include(a => a.AbsenceCategory)
                    .Include(a => a.Document)
                    .FirstOrDefaultAsync(a => a.AbsenceId == absenceId);
                if (absence == null)
                {
                    return false;
                }

                _context.Document.RemoveRange(absence.Document);
                _context.Absences.Remove(absence);

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<Absence> GetAbsenceById(int absenceId)
        {
            try
            {
                var absence = await _context.Absences
                    .Include(a => a.User)
                    .Include(a => a.AbsenceCategory)
                    .Include(a => a.Document)  // Incluye los documentos
                    .FirstOrDefaultAsync(a => a.AbsenceId == absenceId);

                return absence;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<AbsenceDetailsViewModel> GetAbsenceDetails(int id)
        {
            var absence = await _context.Absences
                .Include(a => a.User)
                .Include(a => a.AbsenceCategory)
                .Include(a => a.Document)
                .FirstOrDefaultAsync(a => a.AbsenceId == id);

            if (absence == null)
            {
                return null;
            }

            var documents = absence.Document.Select(d => new DocumentViewModel
            {
                Id = d.Id,
                FileName = d.FileName,
                ContentType = d.ContentType,
                Size = d.Size,
                Content = d.Content
            }).ToList();

            var model = new AbsenceDetailsViewModel
            {
                Category = absence.AbsenceCategory.Category_Name,
                StartDate = absence.Start_Date.Value,
                EndDate = absence.End_Date.Value,
                Comments = absence.Absence_Comments,
                Document = documents
            };

            return model;
        }


        public async Task<DocumentViewModel> DownloadDocument(int documentId)
        {
            var document = await _context.Document.FindAsync(documentId);
            if (document == null)
            {
                return null;
            }

            var documentViewModel = new DocumentViewModel
            {
                Id = document.Id,
                FileName = document.FileName,
                ContentType = document.ContentType,
                Size = document.Size,
                Content = document.Content
            };

            return documentViewModel;
        }

        public async Task<IList<Absence>> GetAbsencesByUser(string userId)
        {
            return await _context.Absences
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<Absence>> GetMyAbsences(string userId)
        {
            try
            {
                return await _context.Absences
                    .Include(u => u.User)
                    .Include(a => a.AbsenceCategory)
                    .Where(o => o.User.Id == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Absence>();
            }
        }
    }
}