using SGRH.Web.Models.Entities;
using SGRH.Web.Models.ViewModels;

namespace SGRH.Web.Services
{
    public interface ILayoffsService
    {
        Task<IList<Layoff>> GetLayoffs();
        Task<(bool success, string message)> CreateLayoff(CreateLayoffViewModel model);
        Task<Layoff> GetLayoffById(int layoffId);
        Task<(bool success, string message)> EditLayoff(int layoffId, UpdateLayoffViewModel model);
        Task<(bool success, string message)> DeleteLayoff(int layoffId);
        Task<int> GetLayoffsCount();
    }
}
