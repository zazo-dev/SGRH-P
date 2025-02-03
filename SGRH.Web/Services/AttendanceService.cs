
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGRH.Web.Models;
using SGRH.Web.Models.Data;
using SGRH.Web.Models.Entities;
using SGRH.Web.Models.ViewModels;
using System.Security.Claims;

namespace SGRH.Web.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly SgrhContext _context;
        private readonly UserManager<User> _userManager;

        public AttendanceService(SgrhContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<Attendance>> GetAttendances(ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);
            var currentUser = await _userManager.GetUserAsync(user);
            var userRole = await _userManager.GetRolesAsync(currentUser);

            if (user.IsInRole("Empleado"))
            {
                return await _context.Attendances
                    .Include(u => u.User)
                    .Where(x => x.UserId == userId)
                    .ToListAsync();
            }

            if (user.IsInRole("SupervisorDpto"))
            {
                return await _context.Attendances
                    .Include(u => u.User)
                    .Where(u => u.User.DepartmentId == currentUser.DepartmentId)
                    .ToListAsync();
            }

            return await _context.Attendances
                    .Include(u => u.User)
                    .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetAttendancesByUser(string userId)
        {
            return await _context.Attendances
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetAttendancesByDate(DateTime startDate, DateTime endDate)
        {
            return await _context.Attendances
                .Where(d => d.Date >= startDate && d.Date <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetAttendancesByUserAndDateRange(string userId, DateTime startDate, DateTime endDate)
        {
            return await _context.Attendances
                .Where(a => a.UserId == userId && a.Date >= startDate && a.Date <= endDate)
                .ToListAsync();
        }

        public async Task<bool> HasEntryForToday(string userId)
        {
            try
            {
                
                DateTime today = DateTime.Today;

                bool hasEntry = await _context.Attendances
                    .AnyAsync(a => a.UserId == userId && a.Date.Date == today);

                return hasEntry;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> RegisterEntry(string userId)
        {
            try
            {
                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
                DateTime currentDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cstZone);
                bool hasEntryForToday = await HasEntryForToday(userId);

                if (hasEntryForToday)
                {
                    return false;
                }

                var attendance = new Attendance
                {
                    UserId = userId,
                    EntryTime = currentDate,
                    Date = currentDate.Date,
                };

                _context.Attendances.Add(attendance);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> RegisterExit(string userId)
        {
            try
            {
                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
                DateTime currentDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cstZone);

                var attendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.UserId == userId &&
                                              EF.Functions.DateDiffDay(a.Date, currentDate) == 0);

                if (attendance == null)
                {
                    return false;
                }

                attendance.ExitTime = currentDate;
                _context.Update(attendance);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<Attendance>> GetMyAttendance(string userId)
        {
            try
            {
                return await _context.Attendances
                    .Include(u=>u.User)
                    .Where(o => o.User.Id == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Attendance>();
            }
        }

    }
}
