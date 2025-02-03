using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGRH.Web.Enums;
using SGRH.Web.Models.Data;
using SGRH.Web.Models.Entities;
using SGRH.Web.Models.ViewModels;
using System.Globalization;
using System.Security.Claims;

namespace SGRH.Web.Services
{

    public class OverTimeService : IOvertimeService
    {
        private readonly SgrhContext _context;
        private readonly UserManager<User> _userManager;

        public OverTimeService(SgrhContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public async Task<(bool success, string message)> CreateOvertime(PersonalAction personalAction, DateTime otDate, int hoursWorked, TypeOT typeOT, decimal salaryPerHour)
        {
            try
            {

                var existingOvertime = await _context.Overtimes.FirstOrDefaultAsync(o => o.OT_Date.Date == otDate.Date && o.PersonalAction.User.Id == personalAction.User.Id);
                if (existingOvertime != null)
                {
                    return (false, "Ya existe un registro de horas extra para esta fecha.");
                }

                var overtime = new Overtime
                {
                    PersonalAction = personalAction,
                    WorkPeriodName = personalAction.User.workPeriod.PeriodName,
                    OT_Date = otDate,
                    Hours_Worked = hoursWorked,
                    TypeOT = typeOT
                };

                decimal amountOT;
                if (typeOT == TypeOT.Sencillas)
                {
                    amountOT = (decimal)salaryPerHour * hoursWorked;
                }
                else
                {
                    amountOT = (decimal)salaryPerHour * 2 * hoursWorked; // Dobles
                }

                overtime.AmountOT = amountOT;

                _context.Overtimes.Add(overtime);

                await _context.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, "Ocurrió un error inesperado durante la solicitud de horas extra");
            }
        }

        public async Task<(bool success, string message)> UpdateOvertime(int overtimeId, PersonalAction personalAction, DateTime otDate, int hoursWorked, TypeOT typeOT, decimal salaryPerHour)
        {
            try
            {
                var overtime = await _context.Overtimes.FirstOrDefaultAsync(o => o.Id_OT == overtimeId);
                if (overtime == null)
                {
                    return (false, "No se encontró el registro de horas extra para actualizar.");
                }

                overtime.PersonalAction = personalAction;
                overtime.WorkPeriodName = personalAction.User.workPeriod.PeriodName;
                overtime.OT_Date = otDate;
                overtime.Hours_Worked = hoursWorked;
                overtime.TypeOT = typeOT;

                decimal amountOT;
                if (typeOT == TypeOT.Sencillas)
                {
                    amountOT = (decimal)salaryPerHour * hoursWorked;
                }
                else
                {
                    amountOT = (decimal)salaryPerHour * 2 * hoursWorked;
                }

                overtime.AmountOT = amountOT;

                _context.Overtimes.Update(overtime);

                await _context.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, "Ocurrió un error inesperado durante la actualización de horas extra");
            }
        }

        public async Task<Overtime> GetOvertimeById(int overtimeId)
        {
            var overtime = await _context.Overtimes
                .Include(p => p.PersonalAction)
                .FirstOrDefaultAsync(o => o.Id_OT == overtimeId);

            return overtime;
        }


        // Lista las horas extra del usuario logueado
        public async Task<IList<Overtime>> GetOvertimes(ClaimsPrincipal user)
        {
            try
            {
                var userId = _userManager.GetUserId(user);
                var currentUser = await _userManager.GetUserAsync(user);
                var userRole = await _userManager.GetRolesAsync(currentUser);

                if (user.IsInRole("Empleado"))
                {
                    return await _context.Overtimes
                        .Where(o => o.PersonalAction.User.Id == userId)
                        .Include(o => o.PersonalAction)
                        .Include(o => o.PersonalAction.User)
                        .Include(o => o.PersonalAction.User.workPeriod)
                        .ToListAsync();
                }

                if (user.IsInRole("SupervisorDpto"))
                {
                    return await _context.Overtimes
                        .Where(o => o.PersonalAction.User.DepartmentId == currentUser.DepartmentId && o.PersonalAction.User.Id != userId)
                        .Include(o => o.PersonalAction)
                        .Include(a => a.PersonalAction.User)
                        .Include(o => o.PersonalAction.User.workPeriod)
                        .ToListAsync();
                }

                if (user.IsInRole("SupervisorRH"))
                {
                    return await _context.Overtimes
                        .Where(o => o.PersonalAction.User.Id != userId)
                        .Include(o => o.PersonalAction)
                        .Include(a => a.PersonalAction.User)
                        .Include(o => o.PersonalAction.User.workPeriod)
                        .ToListAsync();
                }

                return await _context.Overtimes
                    .Include(o => o.PersonalAction)
                    .Include(o => o.PersonalAction.User)
                    .Include(o => o.PersonalAction.User.workPeriod)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Overtime>();
            }
        }

        public async Task<List<Overtime>> GetMyOvertimes(string userId)
        {
            try
            {
                return await _context.Overtimes
                 .Include(o => o.PersonalAction)
                 .ThenInclude(pa => pa.User)
                 .Where(o => o.PersonalAction != null && o.PersonalAction.User.Id == userId)
                 .ToListAsync();


            }
            catch (Exception ex)
            {
                return new List<Overtime>();
            }
        }

        // Lista las horas extra en función al estado que tengan
        public async Task<IList<Overtime>> OvertimeRequests(Enums.Status status, ClaimsPrincipal user)
        {
            try
            {
                var userId = _userManager.GetUserId(user);
                var currentUser = await _userManager.GetUserAsync(user);
                var userRole = await _userManager.GetRolesAsync(currentUser);

                
                if (user.IsInRole("SupervisorDpto"))
                {

                    return await _context.Overtimes
                        .Where(o => o.PersonalAction.Status == status && o.PersonalAction.User.DepartmentId == currentUser.DepartmentId && o.PersonalAction.User.Id != userId)
                        .Include(o => o.PersonalAction)
                        .Include(a => a.PersonalAction.User)
                        .ToListAsync();

                }

                if (user.IsInRole("SupervisorRH"))

                {
                    return await _context.Overtimes
                        .Where(o => o.PersonalAction.Status == status && o.PersonalAction.User.Id != userId)
                        .Include(o => o.PersonalAction)
                        .Include(a => a.PersonalAction.User)
                        .ToListAsync();

                }

                return await _context.Overtimes
                    .Where(o => o.PersonalAction.Status == status)
                    .Include(o => o.PersonalAction)
                    .Include(a => a.PersonalAction.User)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Overtime>();
            }
        }

        //Funcionn para aprobar una solicitud de horas extra
        public async Task<(bool success, string message)> ApproveOvertimeRequest(int overtimeId, ClaimsPrincipal approvalUser) 
        {
            try
            {
                // Obtener la solicitud de horas extra
                var overtime = await _context.Overtimes
                    .Include(o => o.PersonalAction)
                    .Include(u => u.PersonalAction.User)
                    .FirstOrDefaultAsync(o => o.Id_OT == overtimeId);

                // Verificar si la solicitud existe
                if (overtime == null)
                {
                    return (false, "La solicitud indicada no existe en el sistema");
                }

                // Obtener el usuario asociado a la solicitud
                var user = await _userManager.FindByIdAsync(overtime.PersonalAction.User.Id);

                //Obtiene id de la jefatura para almacenarlo en la acción del personal
                var approvalUserId = _userManager.GetUserId(approvalUser);
                var approvalU = await _context.Users.FirstOrDefaultAsync(u => u.Id == approvalUserId);

                // Verificar si el usuario que aprueba la solicitud es el mismo que la creó
                if (user.Id == approvalUserId)
                {
                    return (false, "No puedes aprobar tu propia solicitud.");
                }

                // Marcar la solicitud como aprobada
                overtime.PersonalAction.Is_Approved = true;
                overtime.PersonalAction.ApprovedByUser = approvalU;
                overtime.PersonalAction.Approval_Date = DateTime.UtcNow;
                overtime.PersonalAction.Status = Enums.Status.Aprobado;
                _context.Overtimes.Update(overtime);

                // Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                // Manejo de errores aquí
                return (false, "Se presentó un error inesperado al aprobar la solicitud");
            }
        }

        //Funcionn para rechazar una solicitud de horas extra
        public async Task<(bool success, string message)> RejectOvertimeRequest(int overtimeId, ClaimsPrincipal rejectionUser)
        {
            try
            {
                // Obtener la solicitud de horas extra
                var overtime = await _context.Overtimes
                    .Include(o => o.PersonalAction)
                    .Include(u => u.PersonalAction.User)
                    .FirstOrDefaultAsync(o => o.Id_OT == overtimeId);

                // Obtener el usuario que rechaza la solicitud
                var rejectionUserId = _userManager.GetUserId(rejectionUser);
                var rejectionU = await _context.Users.FirstOrDefaultAsync(u => u.Id == rejectionUserId);

                // Verificar si el usuario que rechaza la solicitud es el mismo que la creó
                if (overtime.PersonalAction.User.Id == rejectionUserId)
                {
                    return (false, "No puedes rechazar tu propia solicitud.");
                }

                // Marcar la solicitud como rechazada
                overtime.PersonalAction.Status = Enums.Status.Rechazado;
                _context.Overtimes.Update(overtime);
                _context.PersonalActions.Update(overtime.PersonalAction);

                // Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                // Manejo de errores aquí
                return (false, "Se presentó un error inesperado al rechazar la solicitud");
            }
        }

        public async Task<(decimal totalOtHours, decimal totalOtAmount)> GetTotalOvertimeForPayrollPeriod(string userId, int payrollPeriodId)
        {
            var totalOtHours = await _context.Overtimes
                .Include(o => o.PersonalAction)
                .Include(o => o.PersonalAction.User)
                .Where(o => o.PersonalAction.User.Id == userId && o.PersonalAction.Status == Status.Aprobado &&
                            o.PersonalAction.CreatedDate >= _context.PayrollPeriod.FirstOrDefault(p => p.Id == payrollPeriodId).StartDate &&
                            o.PersonalAction.CreatedDate <= _context.PayrollPeriod.FirstOrDefault(p => p.Id == payrollPeriodId).EndDate)
                .SumAsync(o => o.Hours_Worked);

            var totalOtAmount = await _context.Overtimes
                .Include(o => o.PersonalAction)
                .Include(o => o.PersonalAction.User)
                .Where(o => o.PersonalAction.User.Id == userId && o.PersonalAction.Status == Status.Aprobado &&
                            o.PersonalAction.CreatedDate >= _context.PayrollPeriod.FirstOrDefault(p => p.Id == payrollPeriodId).StartDate &&
                            o.PersonalAction.CreatedDate <= _context.PayrollPeriod.FirstOrDefault(p => p.Id == payrollPeriodId).EndDate)
                .SumAsync(o => o.AmountOT);

            return (totalOtHours, totalOtAmount);
        }

        public async Task<int> GetOvertimeCount(string userId)
        {
            var count = await _context.Overtimes
                .Where(o => o.PersonalAction.User.Id == userId && o.PersonalAction.Status == Status.Aprobado)
                .CountAsync();

            return count;
        }

        public async Task<decimal> GetTotalApprovedOvertimeAmount(string userId)
        {
            var totalAmount = await _context.Overtimes
                .Where(o => o.PersonalAction.User.Id == userId && o.PersonalAction.Status == Enums.Status.Aprobado)
                .SumAsync(o => o.AmountOT);

            return totalAmount;
        }

        public async Task<List<(string Month, decimal Amount)>> GetMonthlyOvertimeAmount(string userId)
        {
            var overtimeAmounts = await _context.Overtimes
                .Where(o => o.PersonalAction.User.Id == userId && o.PersonalAction.Status == Enums.Status.Aprobado)
                .GroupBy(o => new { Year = o.OT_Date.Year, Month = o.OT_Date.Month })
                .Select(g => new
                {
                    Month = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month)} {g.Key.Year}",
                    Amount = g.Sum(o => o.AmountOT)
                })
                .ToListAsync();

            return overtimeAmounts.Select(a => (a.Month, a.Amount)).ToList();
        }



    }
}
