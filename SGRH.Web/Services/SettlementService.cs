using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGRH.Web.Models.Data;
using SGRH.Web.Models.Entities;
using SGRH.Web.Models.ViewModels;
using System.Globalization;

namespace SGRH.Web.Services
{
    public class SettlementService : ISettlementService
    {
        private readonly SgrhContext _context;
        private readonly UserManager<User> _userManager;

        public SettlementService(SgrhContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IList<Settlement>> GetSettlements()
        {
            try
            {
                return await _context.Settlements
                .Include(l => l.Layoff)
                .Include(l => l.Layoff.PersonalAction)
                .Include(l => l.Layoff.PersonalAction.User)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Settlement>();
            }
        }

        public async Task<Settlement> GetSettlementById(int Settlementid) 
        {
            return await _context.Settlements
                .Include(i => i.Layoff)
                    .ThenInclude(i => i.PersonalAction.User)
                .FirstOrDefaultAsync(s => s.Id == Settlementid);
        }

        public async Task<(bool success, string message)> CreateSettlement(CreateSettlementViewModel model)
        {
            try
            {
                if (model == null)
                {
                    return (false, "Lo sentimos, no es posible procesar la liquidación.");
                }

                // Verificar si el usuario es el mismo que está intentando crear la liquidación
                var layoff = await _context.Layoffs
                    .Include(l => l.PersonalAction)
                        .ThenInclude(pa => pa.User)
                    .FirstOrDefaultAsync(l => l.Id == model.LayoffId);

                if (layoff == null)
                {
                    return (false, "El despido asociado no existe.");
                }

                if (layoff.PersonalAction.User.Id == model.currentUserId)
                {
                    return (false, "No es posible registrar una liquidación para su usuario.");
                }

                // Verificar si ya existe una liquidación para el mismo usuario
                var existingSettlement = await _context.Settlements.FirstOrDefaultAsync(s => s.LayoffId == model.LayoffId);
                if (existingSettlement != null)
                {
                    return (false, "Ya existe una liquidación registrada para este usuario.");
                }

                var settlement = new Settlement
                {
                    AvgLast6MonthsSalary = model.AvgLast6MonthsSalary,
                    DailyAvgLast6Months = model.DailyAvgLast6Months,
                    Bonus = model.Bonus,
                    LayoffId = model.LayoffId,
                    UnenjoyedVacation = model.UnenjoyedVacation,
                    UnenjoyedVacationAmount = model.UnenjoyedVacationAmount,
                    Notice = model.Notice,
                    NoticeAmount = model.NoticeAmount,
                    Severance = model.Severance,
                    SeveranceAmount = model.SeveranceAmount,
                    TotalSettlement = model.TotalSettlement,
                    SettlementDate = DateTime.Now
                };

                // Actualizar HasProcessed en Layoffs

                if (layoff != null)
                {
                    layoff.HasProcessed = true;
                }

                _context.Settlements.Add(settlement);
                await _context.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, "Ocurrió un error inesperado durante el registro de la liquidación.");
            }
        }

        public async Task<(bool success, string message)> DeleteSettlement(int settlementId)
        {
            try
            {
                var settlement = await _context.Settlements
                    .Include(s => s.Layoff)
                    .FirstOrDefaultAsync(s => s.Id == settlementId);

                if (settlement == null)
                {
                    return (false, "No se encontró la liquidación.");
                }

                // Marcar el Layoff asociado como no procesado
                settlement.Layoff.HasProcessed = false;

                _context.Settlements.Remove(settlement);
                await _context.SaveChangesAsync();

                return (true, "La liquidación fue eliminada correctamente.");
            }
            catch (Exception ex)
            {
                return (false, "Ocurrió un error al eliminar la liquidación: " + ex.Message);
            }
        }

        public async Task<int> GetSettlementsCount()
        {
            return await _context.Settlements.CountAsync();
        }

        public async Task<decimal> GetTotalSettlementAmount()
        {
            var totalAmount = await _context.Settlements
                .Select(s => s.TotalSettlement)
                .SumAsync();

            return totalAmount;
        }

        public async Task<List<(string Month, decimal Amount)>> GetMonthlySettlementAmount()
        {
            var settlementAmounts = await _context.Settlements
                .GroupBy(s => new { Year = s.SettlementDate.Year, Month = s.SettlementDate.Month })
                .Select(g => new
                {
                    Month = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month)} {g.Key.Year}",
                    Amount = g.Sum(s => s.TotalSettlement)
                })
                .ToListAsync();

            return settlementAmounts.Select(a => (a.Month, a.Amount)).ToList();
        }


    }
}
