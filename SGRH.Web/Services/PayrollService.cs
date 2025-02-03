using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using SGRH.Web.Enums;
using SGRH.Web.Models;
using SGRH.Web.Models.Data;
using SGRH.Web.Models.Entities;
using SGRH.Web.Models.ViewModels;
using System.Globalization;
using System.Net;
using System.Security.Claims;

namespace SGRH.Web.Services
{
    public class PayrollService : IPayrollService
    {

        private readonly SgrhContext _context;
        private readonly UserManager<User> _userManager;

        public PayrollService(SgrhContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<(bool success, string message)> CreatePayroll(CreatePayrollViewModel model)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);

                var newPayroll = new Payroll
                {
                    User = user,
                    PayrollPeriodId = (int)model.PayrollPeriodId,
                    PayrollFrequency = (PayrollFrequency)model.PayrollFrequency,
                    OrdinarySalary = model.OrdinarySalary,
                    OtHours = model.OtHours,
                    OtHoursAmount = model.OtHoursAmount,
                    BancoPopular = model.BancoPopular,
                    EnfermedadMaternidad = model.EnfermedadMaternidad,
                    IVM = model.IVM,
                    TotalDeductions = model.TotalDeductions,
                    GrossSalary = model.GrossSalary,
                    NetSalary = model.NetSalary,
                    PaymentDate = DateTime.Now
                };

                // Agregar la nueva nómina a la base de datos
                _context.Payrolls.Add(newPayroll);
                await _context.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, "Ocurrió un error al intentar crear la nómina.");
            }
        }

        public async Task<IList<Payroll>> GetAllPayrolls(ClaimsPrincipal user)
        {
            try
            {
                return await _context.Payrolls
                .Include(x => x.PayrollPeriod)
                .Include(u => u.User)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Payroll>();
            }
        }
        public async Task<IList<Payroll>> GetPersonalPayrolls(string userId)
        {
            try
            {
                return await _context.Payrolls
                    .Include(x => x.PayrollPeriod)
                    .Include(u => u.User)
                    .Where(p => p.User.Id == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Payroll>();
            }
        }


        public async Task<Payroll> GetPayrollById(int id)
        {
            try
            {
                var payroll = await _context.Payrolls
                    .Include(x => x.PayrollPeriod)
                    .Include(p => p.User)
                        .ThenInclude(u => u.workPeriod) // Incluye la relación con la tabla WorkPeriod
                    .Include(p => p.User)
                        .ThenInclude(u => u.Department) // Incluye la relación con la tabla Department
                    .FirstOrDefaultAsync(p => p.Id_Payroll == id);

                return payroll;
            }
            catch (Exception ex)
            {
                // Aquí puedes manejar el error como prefieras
                return null;
            }
        }

        [Authorize(Roles = "SupervisorRH")]
        public async Task<(bool success, string message)> UpdatePayroll(UpdatePayrollViewModel model)
        {
            try
            {
                var payroll = await _context.Payrolls.FirstOrDefaultAsync(p => p.Id_Payroll == model.Id_Payroll);

                if (payroll == null)
                {
                    return (false, "La nómina que intentas actualizar no existe.");
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);

                payroll.User = user;
                payroll.PayrollPeriodId = (int)model.PayrollPeriodId;
                payroll.PayrollFrequency = (PayrollFrequency)model.PayrollFrequency;
                payroll.OrdinarySalary = model.OrdinarySalary;
                payroll.OtHours = model.OtHours;
                payroll.OtHoursAmount = model.OtHoursAmount;
                payroll.BancoPopular = model.BancoPopular;
                payroll.EnfermedadMaternidad = model.EnfermedadMaternidad;
                payroll.IVM = model.IVM;
                payroll.TotalDeductions = model.TotalDeductions;
                payroll.GrossSalary = model.GrossSalary;
                payroll.NetSalary = model.NetSalary;

                await _context.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, "Ocurrió un error al intentar actualizar la nómina.");
            }
        }

        [Authorize(Roles = "SupervisorRH")]
        public async Task<(bool success, string message)> DeletePayroll(int id)
        {
            try
            {
                var payroll = await _context.Payrolls.FirstOrDefaultAsync(p => p.Id_Payroll == id);

                if (payroll == null)
                {
                    return (false, "La nómina que intentas eliminar no existe.");
                }

                _context.Payrolls.Remove(payroll);
                await _context.SaveChangesAsync();

                return (true, "Nómina eliminada exitosamente.");
            }
            catch (Exception ex)
            {
                return (false, "Ocurrió un error al intentar eliminar la nómina.");
            }
        }


        public async Task<(decimal avgMonthly, decimal avgDaily)> CalculateAvgLast6MonthsSalary(string userId)
        {

            DateTime currentDate = DateTime.Now;
            DateTime sixMonthsAgoDate;

            if (currentDate.Month >= 6)
            {
                sixMonthsAgoDate = new DateTime(currentDate.Year, currentDate.Month - 5, 1);
            }
            else
            {
                sixMonthsAgoDate = new DateTime(currentDate.Year - 1, 12 + currentDate.Month - 5, 1);
            }

            var payrollPeriods = await _context.PayrollPeriod
                .Where(p => p.StartDate >= sixMonthsAgoDate && p.EndDate <= currentDate)
                .ToListAsync();

            // Calcular el promedio de los salarios para cada periodo
            List<decimal> salaries = new List<decimal>();
            List<bool> isQuincenalList = new List<bool>();
            foreach (var period in payrollPeriods)
            {
                // Obtener los salarios asociados al periodo para el empleado específico
                var payroll = await _context.Payrolls
                    .Where(p => p.User.Id == userId && p.PayrollPeriodId == period.Id)
                    .Select(p => new { p.GrossSalary, p.PayrollFrequency })
                    .ToListAsync();

                // Determinar si la nómina es quincenal
                bool isQuincenal = payroll.Any(p => p.PayrollFrequency == PayrollFrequency.Quincenal);
                if (isQuincenal) { isQuincenalList.Add(isQuincenal); }
                
                // Agregar los salarios al listado para calcular el promedio luego
                salaries.AddRange(payroll.Select(p => p.GrossSalary));
            }

            // Calcular el promedio de los salarios
            decimal avgMonthly = 0;
            decimal avgDaily = 0;

            if (salaries.Any())
            {
                if (isQuincenalList.Any(q => q))
                {
                    //// Para nóminas quincenales, dividir la cantidad de periodos válidos entre 2
                    //int validPeriodsCount = salaries.Count(p => p > 0);
                    //validPeriodsCount = Math.Max(validPeriodsCount, 1); // Garantizar que haya al menos un periodo válido
                    //avgMonthly = salaries.Sum() / (validPeriodsCount * 2); // Cada 2 periodos representan un mes

                    //int validMonthsCount = (int)Math.Ceiling((decimal)salaries.Count / 2); // Dividir entre 2 y redondear hacia arriba
                    decimal validMonthsCount = (decimal)salaries.Count / 2;
                    validMonthsCount = Math.Max(validMonthsCount, 1); // Garantizar que haya al menos un mes válido

                    // Calcular el promedio mensual
                     avgMonthly = salaries.Sum() / validMonthsCount;
                }
                else
                {
                    // Para nóminas mensuales, no es necesario dividir
                    int validMonthsCount = salaries.Count(p => p > 0);
                    validMonthsCount = Math.Max(validMonthsCount, 1); // Garantizar que haya al menos un mes válido
                    avgMonthly = salaries.Sum() / validMonthsCount;
                }

                // Calcular el promedio diario
                avgDaily = avgMonthly / 30;

                avgDaily = Math.Round(avgDaily, 2);
            }

            return (avgMonthly, avgDaily);
        }

        public async Task<bool> HasEnoughDataForAvgSalaryCalculation(string userId)
        {
            DateTime currentDate = DateTime.Now;
            DateTime sixMonthsAgoDate;

            if (currentDate.Month >= 6)
            {
                sixMonthsAgoDate = new DateTime(currentDate.Year, currentDate.Month - 5, 1);
            }
            else
            {
                sixMonthsAgoDate = new DateTime(currentDate.Year - 1, 12 + currentDate.Month - 5, 1);
            }

            var payrollPeriods = await _context.PayrollPeriod
                .Where(p => p.StartDate >= sixMonthsAgoDate && p.EndDate <= currentDate)
                .ToListAsync();

            // Verificar si hay suficientes periodos de pago en los últimos 6 meses
            return payrollPeriods.Count >= 12;
        }

        public async Task<PayrollDetailsViewModel> GetPayrollDetailsForEmployee(int payrollId)
        {
            try
            {
                var payroll = await _context.Payrolls
                    .Include(p => p.User)
                    .Include(p => p.User.Department)
                    .Include(p => p.User.workPeriod)
                    .FirstOrDefaultAsync(p => p.Id_Payroll == payrollId);

                if (payroll == null)
                {
                    return null; // Maneja el caso en el que no se encuentre la nómina
                }

                var payrollDetails = new PayrollDetailsViewModel
                {
                    FullName = payroll.User.FullName,
                    Department = payroll.User.Department.Department_Name,
                    BaseSalary = payroll.User.BaseSalary,
                    WorkPeriod = payroll.User.workPeriod.PeriodName,
                    PayrollPeriod = payroll.PayrollPeriod.PeriodName,
                    PayrollFrequency = payroll.PayrollFrequency.ToString(),
                    OrdinarySalary = payroll.OrdinarySalary,
                    OtHours = payroll.OtHours,
                    OtHoursAmount = payroll.OtHoursAmount,
                    BancoPopular = payroll.BancoPopular,
                    EnfermedadMaternidad = payroll.EnfermedadMaternidad,
                    IVM = payroll.IVM,
                    TotalDeductions = payroll.TotalDeductions,
                    GrossSalary = payroll.GrossSalary,
                    NetSalary = payroll.NetSalary
                };

                return payrollDetails;
            }
            catch (Exception ex)
            {
                // Maneja cualquier excepción aquí
                return null;
            }
        }

        public async Task<int> GetPayrollCount(string userId)
        {
            int count;
            if(userId is not null)
            {
                count = await _context.Payrolls
                .Where(p => p.User.Id == userId)
                .CountAsync();

            }
            else
            {
                count = await _context.Payrolls
                .CountAsync();
            }

            return count;
        }

        public async Task<decimal> GetTotalPayrollAmount(string userId)
        {
            decimal totalAmount;
            if(userId is not null)
            {
                totalAmount = await _context.Payrolls
                    .Where(p => p.User.Id == userId)
                    .SumAsync(o => o.GrossSalary);
            }
            else
            {
                totalAmount = await _context.Payrolls
                    .SumAsync(o => o.GrossSalary);
            }

            return totalAmount;
        }

        public async Task<List<(string Month, decimal Amount)>> GetMonthlyPayrollAmount(string userId)
        {

            if(userId is not null)
            {
                var payrollAmounts = await _context.Payrolls
                .Where(p => p.User.Id == userId)
                .GroupBy(p => new { Year = p.PayrollPeriod.EndDate.Year, Month = p.PayrollPeriod.EndDate.Month })
                .Select(g => new
                {
                    Month = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month)} {g.Key.Year}",
                    Amount = g.Sum(p => p.GrossSalary)
                })
                .ToListAsync();

                return payrollAmounts.Select(a => (a.Month, a.Amount)).ToList();
            }
            else
            {
                var payrollAmounts = await _context.Payrolls
                .GroupBy(p => new { Year = p.PaymentDate.Year, Month = p.PaymentDate.Month })
                .Select(g => new
                {
                    Month = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month)} {g.Key.Year}",
                    Amount = g.Sum(p => p.GrossSalary)
                })
                .ToListAsync();

                return payrollAmounts.Select(a => (a.Month, a.Amount)).ToList();
            }

        }


    }
}