
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGRH.Web.Models.Data;
using SGRH.Web.Models.Entities;

namespace SGRH.Web.Services
{
    public class VacationIncrementService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly SgrhContext _context;
        private readonly UserManager<User> _userManager;

        public VacationIncrementService(IServiceProvider services, SgrhContext context, UserManager<User> userManager)
        {
            _services = services;
            _context = context;
            _userManager = userManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _services.CreateScope())
                {

                    var employees = await _context.Users.ToListAsync();

                    foreach (var employee in employees)
                    {
                        if (IsAnniversary(employee.HiredDate)) // Check if today is the anniversary day
                        {
                            employee.VacationDays++; // Increment vacation days
                            await _userManager.UpdateAsync(employee); // Save changes
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken); // Wait until tomorrow
            }
        }

        private bool IsAnniversary(DateTime? hireDate)
        {
            if (hireDate.HasValue)
            {
                var today = DateTime.UtcNow;
                return hireDate.Value.Day == today.Day;
            }
            return false;
        }


    }
}
