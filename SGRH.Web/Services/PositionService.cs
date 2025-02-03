using Microsoft.EntityFrameworkCore;
using SGRH.Web.Models.Data;
using SGRH.Web.Models.Entities;

namespace SGRH.Web.Services
{
    public class PositionService : IPositionService
    {
        private readonly SgrhContext _context;

        public PositionService(SgrhContext context) {
            _context = context;
        }

        public async Task<(bool success, string message)> CreatePositions(Position model)
        {
            try
            {
                _context.Positions.Add(model);
                await _context.SaveChangesAsync();

                return (true, "Departamento creado exitosamente.");
            }
            catch (Exception ex)
            {
                return (false, "Ocurrió un error al intentar crear el departamento.");
            }
        }
        public async Task<(bool success, string message)> UpdatePositions(Position model)
        {
            try
            {
                var existingPosition = await _context.Positions.FindAsync(model.Id_Position);

                if (existingPosition == null)
                {
                    return (false, "La posición que intentas actualizar no existe.");
                }

              
                existingPosition.Position_Name = model.Position_Name;
                existingPosition.DepartmentId = model.DepartmentId; 

                await _context.SaveChangesAsync();

                return (true, "Posición actualizada exitosamente.");
            }
            catch (Exception ex)
            {
                return (false, "Ocurrió un error al intentar actualizar la posición.");
            }
        }

        public async Task<(bool success, string message)> DeletePosition(int PositionId)
        {
            try
            {
                var position = await _context.Positions.FirstOrDefaultAsync(d => d.Id_Position == PositionId);

                if (position == null)
                {
                    return (false, "El departamento que intentas eliminar no existe.");
                }

                _context.Positions.Remove(position);
                await _context.SaveChangesAsync();

                return (true, "Departamento eliminado exitosamente.");
            }
            catch (Exception ex)
            {
                return (false, "Ocurrió un error al intentar eliminar el departamento.");
            }
        }

        public async Task<Position> GetPositionById(int id)
        {
            try
            {
                var department = await _context.Positions.FirstOrDefaultAsync(d => d.Id_Position == id);

                return department;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<Position>> GetPositions()
        {
            return await _context.Positions.Include(p => p.Department).ToListAsync();
        }

        public async Task<int> GetPositionsCount()
        {
            return await _context.Positions.CountAsync();
        }
    }
}
