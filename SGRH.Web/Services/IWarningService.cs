using SGRH.Web.Models;
using SGRH.Web.Models.Entities;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SGRH.Web.Services
{
    public interface IWarningService
    {
        Task<(bool success, string errorMessage)> CreateWarning(string supervisorId, WarningViewModel model, PersonalAction personalActionId, string userId);
        Task<List<Warning>> GetWarnings(ClaimsPrincipal user);
        Task<Warning> GetWarningById(int warningId);
        Task<bool> UpdateWarning(int warningId, WarningViewModel model);
        Task<bool> DeleteWarning(int warningId);
        Task<List<Warning>> GetLatestWarnings(ClaimsPrincipal user);
        Task<List<WarningViewModel>> GetLatestNotifications(ClaimsPrincipal user);
        Task<int> GetWarningByUser(string userId);
        Task<int> GetWarningsCount();
        Task<List<Warning>> GetMyWarnings(string userId);
    }
}
