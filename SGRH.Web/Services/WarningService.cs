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
    public class WarningService : IWarningService
    {
        private readonly SgrhContext _context;
        private readonly UserManager<User> _userManager;

        public WarningService(SgrhContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<(bool success, string errorMessage)> CreateWarning(string supervisorId, WarningViewModel model, PersonalAction personalActionId, string userId)
        {
            try
            {
                if (supervisorId.Equals(userId)) {
                    return (false,"Lo sentimos, no puede registrar una amonestación para usted mismo.");
                }

                var supervisor = await _context.Users.FirstOrDefaultAsync(u => u.Id == supervisorId);
                var userWarning = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (supervisor == null)
                {
                    return (false,"Supervisor no encontrado.");
                }

                var warning = new Warning
                {
                    PersonalAction = personalActionId,
                    Reason = model.Reason,
                    Observations = model.Observations,
                    User = userWarning 
                };
                warning.PersonalAction.Is_Approved = true;
                warning.PersonalAction.ApprovedByUser = supervisor;
                warning.PersonalAction.Approval_Date = DateTime.Now;
                warning.PersonalAction.Status = Status.Aprobado;

                _context.Warnings.Add(warning);
                await _context.SaveChangesAsync();

                return (true,null);
            }
            catch (Exception ex)
            {
                return (false, "Ha ocurrido un error inesperado durante el registro de la amonestación.");
            }
        }
        public async Task<List<Warning>> GetLatestWarnings(ClaimsPrincipal user)
        {
            try
            {
                var userId = _userManager.GetUserId(user);

                // Obtener las últimas advertencias del usuario actual
                IQueryable<Warning> latestWarningsQuery = _context.Warnings
                    .Include(w => w.User)
                    .OrderByDescending(w => w.Id_Warnings)
                    .Where(w => w.User.Id == userId)
                    .Take(5); 

                return await latestWarningsQuery.ToListAsync();
            }
            catch (Exception ex)
            {
                
                return new List<Warning>();
            }
        }
        public async Task<List<WarningViewModel>> GetLatestNotifications(ClaimsPrincipal user)
        {
            var currentUserWarnings = await GetLatestWarnings(user);

            var latestNotifications = new List<WarningViewModel>();
            foreach (var warning in currentUserWarnings)
            {
                var notification = new WarningViewModel
                {
                    Id_Warnings = warning.Id_Warnings,
                    Reason = warning.Reason,
                    Observations = warning.Observations
                };
                latestNotifications.Add(notification);
            }

            return latestNotifications;
        }


        public async Task<List<Warning>> GetWarnings(ClaimsPrincipal user)
        {
            try
            {
                var userId = _userManager.GetUserId(user);
                var currentUser = await _userManager.GetUserAsync(user);
                var userRole = await _userManager.GetRolesAsync(currentUser);

                if (user.IsInRole("Empleado"))
                {
                    return await _context.Warnings
                        .Where(o => o.User.Id == userId)
                        .Include(o => o.User)
                        .Include(o => o.PersonalAction)
                        .Include(a => a.PersonalAction.User)
                        .ToListAsync();
                }

                if (user.IsInRole("SupervisorDpto"))
                {
                    return await _context.Warnings
                        .Where(o => o.PersonalAction.User.DepartmentId == currentUser.DepartmentId && o.PersonalAction.User.Id != userId)
                        .Include(o => o.User)
                        .Include(o => o.PersonalAction)
                        .Include(a => a.PersonalAction.User)
                        .ToListAsync();
                }

                if (user.IsInRole("SupervisorRH"))
                {
                    return await _context.Warnings
                        .Where(o => o.PersonalAction.User.Id != userId)
                        .Include(o => o.User)
                        .Include(o => o.PersonalAction)
                        .Include(a => a.PersonalAction.User)
                        .ToListAsync();
                }

                return await _context.Warnings
                    .Include(o => o.User)
                    .Include(o => o.PersonalAction)
                    .Include(o => o.PersonalAction.User)
                    .ToListAsync();

            }
            catch (Exception ex)
            {
                return new List<Warning>();
            }
        }

        public async Task<Warning> GetWarningById(int warningId)
        {
            try
            {
                var warning = await _context.Warnings.Include(w => w.User).FirstOrDefaultAsync(w => w.Id_Warnings == warningId);
                return warning;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> UpdateWarning(int warningId, WarningViewModel model)
        {
            try
            {
                var warning = await _context.Warnings
                    .Include(p => p.PersonalAction)
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(w => w.Id_Warnings == warningId);

                var personalActions = await _context.PersonalActions.FirstOrDefaultAsync(w => w.Id_Action == warning.PersonalAction.Id_Action);

                if (warning == null)
                {
                    return false;
                }

                warning.Reason = model.Reason;
                warning.Observations = model.Observations;
                personalActions.Description = model.Reason;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> DeleteWarning(int warningId)
        {
            try
            {
                var warning = await _context.Warnings
                    .Include(p => p.PersonalAction)
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(w => w.Id_Warnings == warningId);

                    var personalActions = await _context.PersonalActions.FirstOrDefaultAsync(w => w.Id_Action == warning.PersonalAction.Id_Action);

                if (warning == null)
                {
                    return false;
                }

                _context.Warnings.Remove(warning);
                _context.PersonalActions.Remove(personalActions);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<int> GetWarningByUser(string userId)
        {
            var count = await _context.Warnings
            .Where(o => o.User.Id == userId)
            .CountAsync();

            return count;
        }

        public async Task<int> GetWarningsCount()
        {
            var count = await _context.Warnings
            .CountAsync();

            return count;
        }

        public async Task<List<Warning>> GetMyWarnings(string userId)
        {
            try
            {
                    return await _context.Warnings
                        .Where(o => o.User.Id == userId)
                        .Include(o => o.PersonalAction)
                        .Include(a => a.PersonalAction.User)
                        .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Warning>();
            }
        }
    }
}
