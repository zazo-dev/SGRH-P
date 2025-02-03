using SGRH.Web.Models;
using SGRH.Web.Models.Entities;
using SGRH.Web.Models.ViewModels;
using System.Security.Claims;

namespace SGRH.Web.Services
{
    public interface IPayrollService
    {
        Task<IList<Payroll>> GetAllPayrolls(ClaimsPrincipal user);
        Task<IList<Payroll>> GetPersonalPayrolls(string userId);
        Task<Payroll> GetPayrollById(int id);
        Task<(bool success, string message)> CreatePayroll(CreatePayrollViewModel model);
        Task<(bool success, string message)> UpdatePayroll(UpdatePayrollViewModel model);
        Task<(bool success, string message)> DeletePayroll(int id);
        Task<(decimal avgMonthly, decimal avgDaily)> CalculateAvgLast6MonthsSalary(string userId);
        Task<bool> HasEnoughDataForAvgSalaryCalculation(string userId);
        Task<PayrollDetailsViewModel> GetPayrollDetailsForEmployee(int payrollId);
        Task<int> GetPayrollCount(string userId);
        Task<decimal> GetTotalPayrollAmount(string userId);
        Task<List<(string Month, decimal Amount)>> GetMonthlyPayrollAmount(string userId);
    }
}
