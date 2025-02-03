using SGRH.Web.Models.Entities;

namespace SGRH.Web.Services
{
    public interface IDepartmentService
    {
        Task<Department> GetDepartmentById(int id);
        Task<List<Department>> GetDepartments();
        Task<int> GetDepartmentCount();
        Task<(bool success, string message)> CreateDepartment(Department model);
        Task<(bool success, string message)> UpdateDepartment(Department model);
        Task<(bool success, string message)> DeleteDepartment(int id);

    }
}
