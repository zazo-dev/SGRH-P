using Microsoft.AspNetCore.Identity;
using SGRH.Web.Models;
using SGRH.Web.Models.Entities;

namespace SGRH.Web.Services
{
    public interface IServiceUser
    {

        Task<(bool success, string errorMessage)> CreateUser(UserViewModel model, HttpContext httpContext);
        Task<List<User>> GetAllUsersWithDetails();
        Task<List<Department>> GetDepartments();
        Task<List<Position>> GetPositions();
        Task<List<Position>> GetPositionsByDepartment(int departmentId);
        Task<UserViewModel> GetUserViewModelById(string userId);
        Task<bool> UpdateUser(UserViewModel model);
        Task<(bool success, string errorMessage)> DeleteUser(UserViewModel model, string currentUserId);
        Task<UserViewModel> GetCurrentUserProfile();
        Task<String> GetUserData(string userId);
        string GenerateTempPass();
        Task<User> GetUserById(string userId);
        Task<User> FindByDniAsync(string dni);
        Task<List<WorkPeriod>> GetWorkPeriods();
        Task<bool> SetStatus(string id, bool status);
        Task<int> GetEmployeeCountByDepartment(string userId);
        Task<int> GetEmployeeCount();



        //Aún no se usan pero se pueden implementar
        // 17 Servicios para usuarios sin implementar (comentados)

        //Task<User> GetUserById(string id);
        //Task<User> GetUserByEmail(string email);


        //Task<IdentityResult> CreateUser(User user, string password);
        //Task<IdentityResult> ChangePassword(User user, string oldPassword, string newPassword);

        //Task<IdentityResult> UpdateUser(User user);
        //Task<IdentityResult> DeleteUser(User user);

        //Task VerifyRole(string role);
        //Task AssignRole(User user,string roleName);
        //Task<bool> UserInRole(User user, string roleName);

        //Task<SignInResult> LogIn(LoginViewModel model);
        //Task SignOut();

        //Task<List<User>> GetUsersByDepartment(int departmentId);
        //Task<List<User>> GetUsersByPosition(int positionId);

        //Task<IdentityResult> ActivateUser(User user);
        //Task<IdentityResult> DeactivateUser(User user);

        //Task RemoveRole(User user, string roleName);
        //Task UpdateRoles(User user, List<string> roleNames);




    }
}
