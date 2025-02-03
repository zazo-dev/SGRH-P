using Microsoft.EntityFrameworkCore;
using SGRH.Web.Enums;
using SGRH.Web.Models.Data;
using SGRH.Web.Models.Entities;
using System.Runtime.CompilerServices;

namespace SGRH.Web.Services
{
    public interface IPersonalActionService
    {
        Task<bool> CreatePersonalAction<T>(T personalAction) where T : PersonalAction;

        Task<PersonalAction> CreatePersonalAction(ActionType actionType, string description, string userId);
        Task<PersonalAction> UpdatePersonalAction(PersonalAction personalAction);
        Task<List<PersonalAction>> GetPersonalActionsForEmployee(string userId);
        Task<int> GetPendingActionsCountForSupervisor(ActionType actionType, string supervisorId);
        Task<int> GetWarningsBySupervisor(ActionType actionType, string supervisorId);
    }
}
