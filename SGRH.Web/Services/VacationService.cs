using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using SGRH.Web.Enums;
using SGRH.Web.Models.Data;
using SGRH.Web.Models.Entities;
using System.Security.Claims;

namespace SGRH.Web.Services
{
    public class VacationService : IVacationService
    {
        private readonly SgrhContext _context;
        private readonly UserManager<User> _userManager;

        public VacationService(SgrhContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //Realiza la solicitud de vacaciones
        public async Task<(bool success, string message)> CreateVacation(PersonalAction personalActionId, DateTime startDate, DateTime endDate, int requestedDays, string comments)
        {
            try
            {
                // Obtener el usuario
                var user = await _userManager.FindByIdAsync(personalActionId.User.Id);

                // Verificar si el usuario tiene días disponibles
                var availableDays = GetAvailableVacationDays(user);

                if (availableDays >= requestedDays)
                {
                    // Crear la solicitud de vacaciones
                    var vacation = new Vacation
                    {
                        PersonalAction = personalActionId,
                        Start_Date = startDate,
                        End_Date = endDate,
                        Comments = comments,
                        RequestedDays = requestedDays
                    };

                    // Agregar la solicitud de vacaciones
                    _context.Vacations.Add(vacation);

                    // Guardar los cambios en la base de datos
                    await _context.SaveChangesAsync();

                    return (true,null);
                }

                return (false, "Lo sentimos, no cuenta con suficientes días disponibles. Días disponibles: "+ availableDays);
            }
            catch (Exception ex)
            {
                return (false,"Ocurrió un error inesperado durante la solicitud de vacaciones");
            }
        }

        public async Task<int> VacationBalance(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new ArgumentException("Usuario no encontrado.");
            }

            return GetAvailableVacationDays(user);
        }

        //Obtiene los días disponibles por usuario
        private int GetAvailableVacationDays(User user)
        {
            var availableUserDays = user.VacationDays;

            //var takenVacationDays = _context.Vacations
            //    .Include(p => p.PersonalAction)
            //    .Include(u => u.PersonalAction.User)
            //    .Where(v => v.PersonalAction.User.Id == user.Id)
            //    .Where(v => v.PersonalAction.Status == null || v.PersonalAction.Status == Enums.Status.Aprobado)
            //    .Where(v => v.Start_Date.Year == DateTime.Today.Year)
            //    .Sum(v => v.RequestedDays);


            //var availableVacationDays = availableUserDays - takenVacationDays;

            return availableUserDays < 0 ? 0 : availableUserDays;
        }

        //Lista las vacaciones de un empleado en función de su rol
        public async Task<IList<Vacation>> GetVacations()
        {
            try
            {
                return await _context.Vacations
                        .Include(v => v.PersonalAction)
                        .Include(a => a.PersonalAction.User)
                        .ToListAsync();
            }
            catch (Exception ex)
            {
                // Manejo de errores aquí
                return new List<Vacation>();
            }
        }

        public async Task<IList<Vacation>> GetVacationsUser(ClaimsPrincipal user)
        {
            try
            {
                var userId = _userManager.GetUserId(user);
                return await _context.Vacations
                        .Where(v => v.PersonalAction.User.Id == userId)
                        .Include(v => v.PersonalAction)
                        .Include(a => a.PersonalAction.User)
                        .ToListAsync();
            }
            catch (Exception ex)
            {
                // Manejo de errores aquí
                return new List<Vacation>();
            }
        }

        public async Task<IList<Vacation>> GetPersonalRequests(string userId)
        {
            try
            {
                return await _context.Vacations
                    .Where(v => v.PersonalAction.User.Id == userId)
                    .Include(v => v.PersonalAction)
                    .Include(a => a.PersonalAction.User)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Vacation>();
            }
        }

        //Lista las vacaciones en función al estado que tengan
        public async Task<IList<Vacation>> VacationsRequests(Enums.Status status, ClaimsPrincipal user)
        {
            try
            {
                var userId = _userManager.GetUserId(user);
                var currentUser = await _userManager.GetUserAsync(user);
                var userRole = await _userManager.GetRolesAsync(currentUser);

                if (user.IsInRole("SupervisorDpto"))
                {
                    return await _context.Vacations
                        .Where(o => o.PersonalAction.Status == status && o.PersonalAction.User.DepartmentId == currentUser.DepartmentId && o.PersonalAction.User.Id != userId)
                        .Include(o => o.PersonalAction)
                        .Include(a => a.PersonalAction.User)
                        .ToListAsync();
                }

                if (user.IsInRole("SupervisorRH"))
                {
                    return await _context.Vacations
                        .Where(o => o.PersonalAction.Status == status && o.PersonalAction.User.Id != userId)
                        .Include(o => o.PersonalAction)
                        .Include(a => a.PersonalAction.User)
                        .ToListAsync();
                }

                return await _context.Vacations
                    .Where(v => v.PersonalAction.Status == status)
                    .Include(v => v.PersonalAction)
                    .Include(a => a.PersonalAction.User)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                
                return new List<Vacation>();
            }
            
        }

        //Funcionn para aprobar una solicitud de vacaciones
        public async Task<(bool success, string message)> ApproveVacationRequest(int vacationId, ClaimsPrincipal approvalUser)
        {
            try
            {
                // Obtener la solicitud de vacaciones
                var vacation = await _context.Vacations
                    .Include(v => v.PersonalAction)
                    .Include(u => u.PersonalAction.User)
                    .FirstOrDefaultAsync(v => v.Id_Vacation  == vacationId);

                // Verificar si la solicitud existe
                if (vacation == null)
                {
                    return (false,"La solicitud indicada no existe en el sistema");
                }

                // Obtener el usuario asociado a la solicitud
                var user = await _userManager.FindByIdAsync(vacation.PersonalAction.User.Id);

                //Obtiene id de la jefatura para almacenarlo en la acción del personal
                var approvalUserId =  _userManager.GetUserId(approvalUser);
                var approvalU = await _context.Users.FirstOrDefaultAsync(u => u.Id == approvalUserId);

                if (user.Id.Equals(approvalUserId))
                {
                    return (false, "Lo sentimos, no puede aprobarse usted mismo su solicitud.");
                }

                // Verificar si el usuario tiene días disponibles
                var availableDays = GetAvailableVacationDays(user);

                if (availableDays >= vacation.RequestedDays)
                {
                    // Actualizar los días disponibles
                    user.VacationDays -= vacation.RequestedDays;
                    await _userManager.UpdateAsync(user);

                    // Marcar la solicitud como aprobada
                    vacation.PersonalAction.Is_Approved = true;
                    vacation.PersonalAction.ApprovedByUser = approvalU;
                    vacation.PersonalAction.Approval_Date = DateTime.UtcNow;
                    vacation.PersonalAction.Status = Enums.Status.Aprobado;
                    _context.Vacations.Update(vacation);

                    // Guardar los cambios en la base de datos
                    await _context.SaveChangesAsync();

                    return (true,null);
                }

                // No hay suficientes días disponibles
                return (false, "Lo sentimos, no cuenta con suficientes días disponibles para proceder con la solicitud.");
            }
            catch (Exception ex)
            {
                // Manejo de errores aquí
                return (false, "Se presentó un error inesperado al aprobar la solicitud");
            }
        }

        public async Task<(bool success, string message)> RejectVacationRequest(int vacationId, ClaimsPrincipal rejectingUser)
        {
            try
            {

                // Obtener la solicitud de vacaciones
                var vacation = await _context.Vacations
                    .Include(v => v.PersonalAction)
                    .Include(u => u.PersonalAction.User)
                    .FirstOrDefaultAsync(v => v.Id_Vacation == vacationId);

                // Obtener el usuario asociado a la solicitud
                var user = await _userManager.FindByIdAsync(vacation.PersonalAction.User.Id);

                //Obtiene id de la jefatura para almacenarlo en la acción del personal
                var rejectUserId = _userManager.GetUserId(rejectingUser);
                var rejectU = await _context.Users.FirstOrDefaultAsync(u => u.Id == rejectUserId);

                if (user.Id.Equals(rejectUserId))
                {
                    return (false, "Lo sentimos, no puede rechazarse usted mismo su solicitud.");
                }

                vacation.PersonalAction.Status = Enums.Status.Rechazado;

                _context.Vacations.Update(vacation);
                _context.PersonalActions.Update(vacation.PersonalAction);

                // Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();

                return (true, null);

            }
            catch(Exception ex)
            {
                return (false, "Error inesperado al rechazar la solicitud de vacaciones");
            }

        }

        public async Task<(bool success, string errorMessage)> AddInitialVacationDays(string userId, int initialVacationDays)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if(user == null)
                {
                    return (false, "No se encontró el usuaio indicado.");
                }

                user.VacationDays += initialVacationDays;

                _context.Update(user);
                await _context.SaveChangesAsync();

                return (true, null);
            }
            catch(Exception ex)
            {
                return (false, "Error inesperado al agregar el saldo actual de vacaciones.");
            }
        }

        //public async Task IncrementVacationDayMonthly()
        //{
        //    var employees = await _context.Users.ToListAsync();

        //    foreach (var employee in employees)
        //    {
        //        employee.VacationDays++;
        //    }

        //    await _context.SaveChangesAsync();
        //}

        public async Task<int> VacationDaysTaken(string userId)
        {
            int totalDaysTaken = await _context.Vacations
                .Include(p => p.PersonalAction)
                .Where(v => v.PersonalAction.User.Id == userId && v.PersonalAction.Status == Enums.Status.Aprobado)
                .SumAsync(v => v.RequestedDays);

            return totalDaysTaken;
        }

    }
}
