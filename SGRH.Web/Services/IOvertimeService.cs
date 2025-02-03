using SGRH.Web.Enums;
using SGRH.Web.Models.Entities;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SGRH.Web.Services
{
    public interface IOvertimeService
    {
        Task<(bool success, string message)> CreateOvertime(PersonalAction personalAction, DateTime otDate, int hoursWorked, TypeOT typeOT, decimal salaryPerHour);
        Task<(bool success, string message)> UpdateOvertime(int overtimeId, PersonalAction personalAction, DateTime otDate, int hoursWorked, TypeOT typeOT, decimal salaryPerHour);
        Task<Overtime> GetOvertimeById(int overtimeId);
        Task<IList<Overtime>> GetOvertimes(ClaimsPrincipal user);
        Task<IList<Overtime>> OvertimeRequests(Enums.Status status, ClaimsPrincipal user);
        Task<(bool success, string message)> ApproveOvertimeRequest(int overtimeId, ClaimsPrincipal approvalUser);
        Task<(bool success, string message)> RejectOvertimeRequest(int overtimeId, ClaimsPrincipal rejectionUser);
        Task<(decimal totalOtHours, decimal totalOtAmount)> GetTotalOvertimeForPayrollPeriod(string userId, int payrollPeriodId);
        Task<int> GetOvertimeCount(string userId);
        Task<decimal> GetTotalApprovedOvertimeAmount(string userId);
        Task<List<(string Month, decimal Amount)>> GetMonthlyOvertimeAmount(string userId);
        Task<List<Overtime>> GetMyOvertimes(string userId);
    }
}
