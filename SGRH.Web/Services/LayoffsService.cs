using Microsoft.EntityFrameworkCore;
using SGRH.Web.Migrations;
using SGRH.Web.Models.Data;
using SGRH.Web.Models.Entities;
using SGRH.Web.Models.ViewModels;

namespace SGRH.Web.Services
{
    public class LayoffsService : ILayoffsService
    {
        private readonly SgrhContext _context;
        private readonly IPersonalActionService _personalActionService;
        private readonly IServiceUser _serviceUser;

        public LayoffsService(SgrhContext context, IPersonalActionService personalActionService, IServiceUser serviceUser)
        {
            _context = context;
            _personalActionService = personalActionService;
            _serviceUser = serviceUser;
        }


        public async Task<IList<Layoff>> GetLayoffs()
        {
            try
            {
                return await _context.Layoffs
                .Include(p => p.PersonalAction)
                .ThenInclude(p => p.User)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Layoff>();
            }
        }

        public async Task<Layoff> GetLayoffById(int layoffId)
        {
            return await _context.Layoffs
                .Include(l => l.PersonalAction)
                    .ThenInclude(pa => pa.User) 
                .FirstOrDefaultAsync(l => l.Id == layoffId); 
        }


        public async Task<(bool success, string message)> CreateLayoff(CreateLayoffViewModel model)
        {
            try
            {
                if(model == null)
                {
                    return (false, "Lo sentimos, no es posible procesar el despido.");
                }

                // Verificar si el usuario está intentando autodespedirse
                if (model.userId == model.currentUserId)
                {
                    return (false, "No es posible registrar un despido para su usuario.");
                }

                // Verificar si ya existe un despido para el usuario
                var existingLayoff = await _context.Layoffs
                    .AnyAsync(l => l.PersonalAction.User.Id == model.userId);

                if (existingLayoff)
                {
                    return (false, "Ya existe un despido registrado para este usuario.");
                }

                model.PersonalAction = await _personalActionService.CreatePersonalAction(Enums.ActionType.Despidos, model.DismissalCause, model.userId);

                var layoff = new Layoff
                {
                    PersonalAction = model.PersonalAction,
                    HasEmployerResponsibility = model.HasEmployerResponsibility,
                    DismissalDate = model.DismissalDate,
                    DismissalCause = model.DismissalCause,
                    RegisteredBy = model.RegisteredBy
                };

                _context.Layoffs.Add(layoff);

                // Cambiar el estado del usuario a inactivo
                var userStatusChanged = await _serviceUser.SetStatus(model.userId, false);
                if (!userStatusChanged)
                {
                    return (false, "No se pudo cambiar el estado del usuario a inactivo.");
                }

                await _context.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex) 
            {
                return (false, "Ocurrió un error inesperado durante el registro del despido.");
            }
        }

        public async Task<(bool success, string message)> EditLayoff(int layoffId, UpdateLayoffViewModel model)
        {
            try
            {
                var layoff = await _context.Layoffs.FindAsync(layoffId);
                if (layoff == null)
                {
                    return (false, "No se encontró el despido.");
                }

                


                layoff.HasEmployerResponsibility = model.HasEmployerResponsibility;
                layoff.DismissalDate = model.DismissalDate;
                layoff.DismissalCause = model.DismissalCause;

                _context.Layoffs.Update(layoff);
                await _context.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, "Ocurrió un error inesperado al editar el despido.");
            }
        }

        public async Task<(bool success, string message)> DeleteLayoff(int layoffId)
        {
            var layoff = await _context.Layoffs
                .Include(p => p.PersonalAction)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(l => l.Id == layoffId);

            if (layoff != null)
            {

                // Cambiar el estado del usuario a inactivo
                var userStatusChanged = await _serviceUser.SetStatus(layoff.PersonalAction.User.Id, true);
                if (!userStatusChanged)
                {
                    return (false, "No se pudo cambiar el estado del usuario a activo.");
                }

                if (layoff.PersonalAction != null)
                {
                    _context.PersonalActions.Remove(layoff.PersonalAction);
                }

                _context.Layoffs.Remove(layoff);

                await _context.SaveChangesAsync();
                return (true,null);
            }

            return (false, "Ocurrió un error inesperado el eliminar el despido");
        }

        public async Task<int> GetLayoffsCount()
        {
            return await _context.Layoffs.CountAsync();
        }
    }
}
