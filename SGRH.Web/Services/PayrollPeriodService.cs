using Microsoft.EntityFrameworkCore;
using SGRH.Web.Models.Data;
using SGRH.Web.Models.Entities;

namespace SGRH.Web.Services
{
    public class PayrollPeriodService : IPayrollPeriodService
    {
        private readonly SgrhContext _context;

        public PayrollPeriodService(SgrhContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PayrollPeriod>> GetAllPayrollPeriodsAsync()
        {
            int currentYear = DateTime.Now.Year;
            return await _context.PayrollPeriod
                .Where(pp => pp.StartDate.Year == currentYear || pp.EndDate.Year == currentYear)
                .ToListAsync();
        }

        public async Task AddPayrollPeriodAsync(PayrollPeriod payrollPeriod)
        {
            _context.PayrollPeriod.Add(payrollPeriod);
            await _context.SaveChangesAsync();
        }

        public async Task GeneratePayrollPeriodsAsync(int year)
        {
            for (int month = 1; month <= 12; month++)
            {
                DateTime startDate = new DateTime(year, month, 1);
                int daysInMonth = DateTime.DaysInMonth(year, month);

                for (int i = 0; i < 2; i++)
                {
                    int periodStartDay = i * 15 + 1;
                    int periodEndDay = i == 0 ? Math.Min((i + 1) * 15, daysInMonth) : daysInMonth;

                    string periodName = $"{periodStartDay} AL {periodEndDay} {startDate.ToString("MMMM").ToUpper()} {year}";
                    PayrollPeriod payrollPeriod = new PayrollPeriod
                    {
                        PeriodName = periodName,
                        StartDate = startDate.AddDays(i * 15),
                        EndDate = startDate.AddDays(periodEndDay - 1)
                    };

                    await AddPayrollPeriodAsync(payrollPeriod);
                }
            }
        }

        public async Task<PayrollPeriod> GetCurrentPayrollPeriodAsync()
        {
            var currentDate = DateTime.UtcNow.Date; // Obtener la fecha actual en formato UTC

            return await _context.PayrollPeriod
                .Where(p => p.StartDate <= currentDate && p.EndDate >= currentDate)
                .FirstOrDefaultAsync();
        }
    }
}
