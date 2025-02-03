using SGRH.Web.Models.Entities;
using SGRH.Web.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SGRH.Web.Services
{
    public interface IDossierService
    {
        Task<bool> CreateDossier(string supervisorId, DossierViewModel model, List<byte[]> documentContents, List<string> documentFileNames, List<string> documentContentTypes);
        Task<List<Dossier>> GetDossiers(ClaimsPrincipal user);
        Task<DossierDetailsViewModel> GetDossierDetails(int dossierId);
        Task<(bool success, string errorMessage)> UpdateDossier(int dossierId, DossierViewModel model, List<byte[]> documentContent, List<string> documentFileName, List<string> documentContentType);
        Task<bool> DeleteDossier(int dossierId);
        Task<int> GetDossierCountByUser(string userId);
        Task<List<Dossier>> GetMyDossier(string userId);
        Task<bool> DeleteDocument(int documentId);
    }
}