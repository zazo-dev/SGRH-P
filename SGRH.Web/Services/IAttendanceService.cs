using SGRH.Web.Models;
using SGRH.Web.Models.Entities;
using SGRH.Web.Models.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SGRH.Web.Services
{
    public interface IAttendanceService
    {
        Task<bool> RegisterEntry(string userId);
        Task<bool> RegisterExit(string userId);
        Task<bool> HasEntryForToday(string userId);
        Task<IEnumerable<Attendance>> GetAttendances(ClaimsPrincipal user);
        Task<IEnumerable<Attendance>> GetAttendancesByUser(string userId);
        Task<IEnumerable<Attendance>> GetAttendancesByDate(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Attendance>> GetAttendancesByUserAndDateRange(string userId, DateTime startDate, DateTime endDate);
        Task<List<Attendance>> GetMyAttendance(string userId);
    }
}
