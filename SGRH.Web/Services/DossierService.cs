using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGRH.Web.Enums;
using SGRH.Web.Models;
using SGRH.Web.Models.Data;
using SGRH.Web.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SGRH.Web.Services
{
    public class DossierService : IDossierService
    {
        private readonly SgrhContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IServiceUser _serviceUser;

        public DossierService(SgrhContext context, UserManager<User> userManager, IServiceUser serviceUser)
        {
            _context = context;
            _userManager = userManager;
            _serviceUser = serviceUser;
        }

        public async Task<bool> CreateDossier(string supervisorId, DossierViewModel model, List<byte[]> documentContents, List<string> documentFileNames, List<string> documentContentTypes)
        {
            try
            {
                var user = await _serviceUser.GetUserById(model.userId);
                if (user == null)
                {
                    return false;
                }

                var dossier = new Dossier
                {
                    User = user,
                    DocumentType = model.DocumentType,
                    Description = model.Description,
                    Date = model.Date,
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
                        CreatedAt = DateTime.Now,
                    };

                    dossier.Document.Add(document);
                }

                _context.Dossiers.Add(dossier);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<Dossier>> GetDossiers(ClaimsPrincipal user)
        {
            try
            {
                var userId = _userManager.GetUserId(user);
                var currentUser = await _userManager.GetUserAsync(user);
                var userRole = await _userManager.GetRolesAsync(currentUser);

                IQueryable<Dossier> dossiersQuery = _context.Dossiers.Include(d => d.User);

                if (userRole.Contains("Empleado"))
                {
                    dossiersQuery = dossiersQuery.Where(d => d.User.Id == userId);
                }
                else if (userRole.Contains("SupervisorDpto"))
                {
                    dossiersQuery = dossiersQuery.Where(d => d.User.DepartmentId == currentUser.DepartmentId);
                }

                return await dossiersQuery.ToListAsync();
            }
            catch (Exception ex)
            {
                // Manejo de errores aquí
                return new List<Dossier>();
            }
        }

        public async Task<DossierDetailsViewModel> GetDossierDetails(int dossierId)
        {
            try
            {
                var dossier = await _context.Dossiers
                    .Include(d => d.User)
                    .Include(d => d.Document)
                    .FirstOrDefaultAsync(d => d.Id_Record == dossierId);

                if (dossier == null)
                {
                    return null;
                }

                var documents = dossier.Document.Select(d => new DossierDocumentViewModel
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    ContentType = d.ContentType,
                    Size = d.Size,
                    Content = d.Content
                }).ToList();

                var detailsViewModel = new DossierDetailsViewModel
                {
                    DocumentType = dossier.DocumentType,
                    Date = dossier.Date ?? DateTime.MinValue,
                    Description = dossier.Description,
                    Document = documents
                };

                return detailsViewModel;
            }
            catch (Exception ex)
            {
                // Manejo de errores aquí
                return null;
            }
        }

        public async Task<(bool success, string errorMessage)> UpdateDossier(int dossierId, DossierViewModel model, List<byte[]> documentContent, List<string> documentFileName, List<string> documentContentType)
        {
            try
            {
                var dossier = await _context.Dossiers
                    .Include(d => d.Document)
                    .FirstOrDefaultAsync(d => d.Id_Record == dossierId);

                if (dossier == null)
                {
                    return (false, "No se encontró el expediente.");
                }

                // Actualizar los datos del dossier
                dossier.DocumentType = model.DocumentType;
                dossier.Description = model.Description;
                dossier.Date = model.Date;

                // Si se proporciona un nuevo documento, actualizar o agregar según corresponda
                if (model.Documentation != null)
                {
                    foreach (var formFile in model.Documentation)
                    {
                        // Verificar si el documento ya existe
                        var existingDocument = dossier.Document.FirstOrDefault(d => d.FileName == formFile.FileName);

                        if (existingDocument != null)
                        {
                            // Documento ya existente, retornar indicando que ya existe
                            return (false, "El documento ya se encuentra adjunto.");
                        }

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

                        dossier.Document.Add(document);
                    }
                }
                _context.Dossiers.Update(dossier);
                await _context.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, "Error al actualizar el expediente.");
            }
        }


        public async Task<bool> DeleteDossier(int dossierId)
        {
            try
            {
                var dossier = await _context.Dossiers
                    .Include(d => d.Document) // Incluye los documentos relacionados
                    .FirstOrDefaultAsync(d => d.Id_Record == dossierId);

                if (dossier == null)
                {
                    return false;
                }

                // Elimina los documentos asociados
                if (dossier.Document != null && dossier.Document.Any())
                {
                    _context.Document.RemoveRange(dossier.Document);
                }

                // Elimina el dossier
                _context.Dossiers.Remove(dossier);

                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Manejo de errores aquí
                return false;
            }
        }

        public async Task<int> GetDossierCountByUser(string userId)
        {
            var count = await _context.Dossiers
            .Where(o => o.User.Id == userId)
            .CountAsync();

            return count;
        }

        public async Task<List<Dossier>> GetMyDossier(string userId)
        {
            try
            {
                return await _context.Dossiers
                    .Include(d=>d.Document)
                    .Include(u => u.User)
                    .Where(o => o.User.Id == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Dossier>();
            }
        }

        public async Task<bool> DeleteDocument(int documentId)
        {
            try
            {
                var document = await _context.Document.FindAsync(documentId);

                if (document == null)
                {
                    return false;
                }

                _context.Document.Remove(document);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


    }
}