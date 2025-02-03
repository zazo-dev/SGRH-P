using SGRH.Web.Models.Entities;
using System.Security.Claims;

namespace SGRH.Web.Services
{
    public interface IVacationService
    {
        Task<(bool success, string message)> CreateVacation(PersonalAction personalActionId, DateTime startDate, DateTime endDate, int requestedDays, string comments);
        Task<IList<Vacation>> GetVacations();
        Task<IList<Vacation>> GetVacationsUser(ClaimsPrincipal user);
        Task<IList<Vacation>> VacationsRequests(Enums.Status status, ClaimsPrincipal user);
        Task<(bool success, string message)> ApproveVacationRequest(int vacationId, ClaimsPrincipal approvalUser);
        Task<(bool success, string message)> RejectVacationRequest(int vacationId, ClaimsPrincipal rejectingUser);
        Task<int> VacationBalance(string userId);
        Task<(bool success, string errorMessage)> AddInitialVacationDays(string userId, int initialVacationDays);
        Task<int> VacationDaysTaken(string userId);
        //Task IncrementVacationDayMonthly();
    }
}
