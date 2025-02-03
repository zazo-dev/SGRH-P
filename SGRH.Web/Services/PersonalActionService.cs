using Microsoft.EntityFrameworkCore;
using SGRH.Web.Enums;
using SGRH.Web.Models.Data;
using SGRH.Web.Models.Entities;

namespace SGRH.Web.Services
{
    public class PersonalActionService : IPersonalActionService
    {
        private readonly SgrhContext _context;

        public PersonalActionService( SgrhContext context)
        {

            _context = context;
        }

        public async Task<bool> CreatePersonalAction<T>(T personalAction) where T : PersonalAction
        {
            try
            {
                _context.PersonalActions.Add(personalAction);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log error
                return false;
            }
        }

        public async Task<PersonalAction> CreatePersonalAction(ActionType actionType, string description, string userId)
        {
            var personalAction = new PersonalAction
            {
                ActionType = actionType,
                Description = description,
                User = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId),
                CreatedDate = DateTime.Now,
                Is_Approved = false,
                Status = Status.Pendiente

            };

            _context.PersonalActions.Add(personalAction);
            //await _context.SaveChangesAsync();

            return personalAction;
        }

        public async Task<PersonalAction> UpdatePersonalAction(PersonalAction personalAction)
        {
            _context.PersonalActions.Update(personalAction);
            await _context.SaveChangesAsync();

            return personalAction;
        }

        public async Task<List<PersonalAction>> GetPersonalActionsForEmployee(string userId)
        {
            return await _context.PersonalActions
                .Where(pa => pa.User.Id == userId)
                .OrderByDescending(pa => pa.CreatedDate)
                .ToListAsync();
        }

        public async Task<int> GetPendingActionsCountForSupervisor(ActionType actionType, string supervisorId)
        {

            int count;

            if(supervisorId != null)
            {
                var departmentId = await _context.Users
                .Where(u => u.Id == supervisorId)
                .Select(u => u.DepartmentId)
                .FirstOrDefaultAsync();

                if (departmentId == null)
                {
                    throw new Exception("Supervisor no encontrado.");
                }

                count = await _context.PersonalActions
                    .Where(pa => pa.ActionType == actionType && pa.Status == Status.Pendiente && pa.User.DepartmentId == departmentId && pa.User.Id != supervisorId )
                    .CountAsync();
            }
            else
            {
                count = await _context.PersonalActions
                .Where(pa => pa.ActionType == actionType && pa.Status == Status.Pendiente)
                .CountAsync();
            }

            return count;
        }


        public async Task<int> GetWarningsBySupervisor(ActionType actionType, string supervisorId)
        {

            var count = await _context.PersonalActions
                .Where(pa => pa.ActionType == actionType && pa.User.Id == supervisorId)
                .CountAsync();

            return count;
        }

    }
}
