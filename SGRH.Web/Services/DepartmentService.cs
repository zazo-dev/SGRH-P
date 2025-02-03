using Microsoft.EntityFrameworkCore;
using SGRH.Web.Models.Data;
using SGRH.Web.Models.Entities;

namespace SGRH.Web.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly SgrhContext _context;

        public DepartmentService(SgrhContext context)
        {
            _context = context;
        }
        public async Task<int> GetDepartmentCount()
        {
            return await _context.Departments.CountAsync();
        }

        public async Task<(bool success, string message)> CreateDepartment(Department model)
        {
            try
            {
                _context.Departments.Add(model);
                await _context.SaveChangesAsync();

                return (true, "Departamento creado exitosamente.");
            }
            catch (Exception ex)
            {
                return (false, "Ocurrió un error al intentar crear el departamento.");
            }
        }

        public async Task<(bool success, string message)> UpdateDepartment(Department model)
        {
            try
            {
                var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id_Department == model.Id_Department);

                if (department == null)
                {
                    return (false, "El departamento que intentas actualizar no existe.");
                }

                department.Department_Name = model.Department_Name;

                await _context.SaveChangesAsync();

                return (true, "Departamento actualizado exitosamente.");
            }
            catch (Exception ex)
            {
                return (false, "Ocurrió un error al intentar actualizar el departamento.");
            }
        }

        public async Task<(bool success, string message)> DeleteDepartment(int DepartmentId)
        {
            try
            {
                var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id_Department == DepartmentId);

                if (department == null)
                {
                    return (false, "El departamento que intentas eliminar no existe.");
                }

                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();

                return (true, "Departamento eliminado exitosamente.");
            }
            catch (Exception ex)
            {
                return (false, "Ocurrió un error al intentar eliminar el departamento.");
            }
        }

        public async Task<Department> GetDepartmentById(int id) 
        {
            try
            {
                var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id_Department == id);

                return department;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<Department>> GetDepartments() 
        {
            return await _context.Departments.ToListAsync();
        }

    }
}
