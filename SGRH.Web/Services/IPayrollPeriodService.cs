using SGRH.Web.Models.Entities;

namespace SGRH.Web.Services
{
    public interface IPayrollPeriodService
    {
        Task<IEnumerable<PayrollPeriod>> GetAllPayrollPeriodsAsync();
        Task AddPayrollPeriodAsync(PayrollPeriod payrollPeriod);
        Task GeneratePayrollPeriodsAsync(int year);
        Task<PayrollPeriod> GetCurrentPayrollPeriodAsync();
    }
}
