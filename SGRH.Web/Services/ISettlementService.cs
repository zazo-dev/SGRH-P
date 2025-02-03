using SGRH.Web.Models.Entities;
using SGRH.Web.Models.ViewModels;

namespace SGRH.Web.Services
{
    public interface ISettlementService
    {
        Task<IList<Settlement>> GetSettlements();
        Task<(bool success, string message)> CreateSettlement(CreateSettlementViewModel model);
        Task<(bool success, string message)> DeleteSettlement(int settlementId);
        Task<Settlement> GetSettlementById(int Settlementid);
        Task<int> GetSettlementsCount();
        Task<decimal> GetTotalSettlementAmount();
        Task<List<(string Month, decimal Amount)>> GetMonthlySettlementAmount();
    }
}
