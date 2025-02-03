using SGRH.Web.Models.Entities;

namespace SGRH.Web.Services
{
    public interface IPositionService
    {
        Task<int> GetPositionsCount();
        Task<(bool success, string message)> CreatePositions(Position model);
        Task<(bool success, string message)> UpdatePositions(Position model);
        Task<(bool success, string message)> DeletePosition(int PositionId);
        Task<Position> GetPositionById(int id);
        Task<List<Position>> GetPositions();


    }
}
