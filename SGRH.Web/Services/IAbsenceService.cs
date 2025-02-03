using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SGRH.Web.Models;
using SGRH.Web.Models.Entities;
using System.Security.Claims;

namespace SGRH.Web.Services
{
    public interface IAbsenceService
    {
        Task<bool> RegisterAbsence(string userId, AbsenceViewModel model, List<byte[]> documentContent, List<string> documentFileName, List<string> documentContentType);
        Task<bool> UpdateAbsence(string userId, int absenceId, AbsenceViewModel model, List<byte[]> documentContents, List<string> documentFileNames, List<string> documentContentTypes);
        Task<bool> DeleteAbsence(ClaimsPrincipal user, int absenceId);
        Task<List<AbsenceCategory>> GetAbsenceCategories();
        Task<Absence> GetAbsenceById(int absenceId);
        Task<IList<Absence>> GetAbsences(ClaimsPrincipal user);
        Task<AbsenceDetailsViewModel> GetAbsenceDetails(int id);
        Task<DocumentViewModel> DownloadDocument(int documentId);
        Task<IList<Absence>> GetAbsencesByUser(string userId);
        Task<List<Absence>> GetMyAbsences(string userId);
    }
}
