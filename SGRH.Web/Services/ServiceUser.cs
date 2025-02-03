 using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SGRH.Web.Enums;
using SGRH.Web.Models;
using SGRH.Web.Models.Data;
using SGRH.Web.Models.Entities;
using System.Net;
using System.Security.Cryptography;
using System.Text;


namespace SGRH.Web.Services
{

    public class ServiceUser : IServiceUser
    {

        private readonly SgrhContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly EmailService _emailService;

        public ServiceUser(SgrhContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager, EmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        private string GenerateTemporaryPassword(int length = 12)
        {
            const string lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
            const string uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numericChars = "0123456789";
            const string specialChars = "!@#$%^&*()-_=+";

            var validChars = lowercaseChars + uppercaseChars + numericChars + specialChars;

            var randomBytes = new byte[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            var result = new StringBuilder(length);

            // Asegurar que haya al menos un carácter de cada tipo
            result.Append(lowercaseChars[randomBytes[0] % lowercaseChars.Length]);
            result.Append(uppercaseChars[randomBytes[1] % uppercaseChars.Length]);
            result.Append(numericChars[randomBytes[2] % numericChars.Length]);
            result.Append(specialChars[randomBytes[3] % specialChars.Length]);

            // Completar el resto de la contraseña de manera aleatoria
            for (int i = 4; i < length; i++)
            {
                result.Append(validChars[randomBytes[i] % validChars.Length]);
            }

            // Mezclar la cadena resultante para mayor aleatoriedad
            var shuffledResult = new string(result.ToString().OrderBy(c => Guid.NewGuid()).ToArray());

            return shuffledResult;
        }


        public string GenerateTempPass()
        {
            var tempPass = GenerateTemporaryPassword();

            return tempPass;
        }

        public async Task<UserViewModel> GetUserViewModelById(string userId)
        {
            var user = await _userManager.Users
            .Include(u => u.Department)
            .Include(u => u.Position)
            .Include(u => u.workPeriod)
            .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return null;
            }

            var model = new UserViewModel
            {
                Id = user.Id,
                Name = user.Name,
                LastName = user.LastName,
                Dni = user.Dni,
                BirthDate = user.BirthDate,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                DepartmentId = user.DepartmentId,
                PositionId = user.PositionId,
                UserType = user.UserType,

                Department = new DepartmentViewModel
                {
                    DepartmentId = user.Department.Id_Department,
                    DepartmentName = user.Department.Department_Name,

                },

                Position = new PositionViewModel
                {
                    PositionId = user.Position.Id_Position,
                    PositionName = user.Position.Position_Name,

                },
                HiredDate = user.HiredDate,
                BaseSalary = user.BaseSalary,
                WorkPeriodId = user.workPeriod.Id,
                WorkPeriod = user.workPeriod,
                IsActive = user.IsActive
            };


            return model;
        }

        public async Task<(bool success, string errorMessage)> CreateUser(UserViewModel model, HttpContext httpContext)
        {
            try
            {
                if (model == null)
                {
                    return (false, "Datos incompletos.");
                }

                string roleName = Enum.GetName(typeof(UserType), model.UserType);

                bool rolExists = await _roleManager.RoleExistsAsync(roleName);

                if (!rolExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }

                // Verificar si ya existe un usuario con el mismo DNI
                var existingUser = await FindByDniAsync(model.Dni);
                if (existingUser != null)
                {
                    return (false, "Ya existe un usuario con el mismo DNI.");
                }

                var workPeriod = await _context.WorkPeriod.FindAsync(model.WorkPeriodId);
                if (workPeriod == null)
                {
                    return (false, "La jornada laboral seleccionada no existe.");
                }

                UserType userType = model.UserType ?? throw new ArgumentNullException(nameof(model.UserType), "El rol no puede ser nulo.");
                decimal baseSalary = model.BaseSalary ?? throw new ArgumentNullException(nameof(model.BaseSalary), "El salario base no puede ser nulo.");
                var newUser = new User
                {
                    Name = model.Name,
                    LastName = model.LastName,
                    Dni = model.Dni,
                    BirthDate = model.BirthDate,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    UserName = model.Email,
                    DepartmentId = model.DepartmentId,
                    PositionId = model.PositionId,
                    UserType = userType,
                    VacationDays = model.VacationDays,
                    HiredDate = model.HiredDate,
                    BaseSalary = baseSalary,
                    workPeriod = workPeriod
                };

                string tempPass = GenerateTemporaryPassword();

                var result = await _userManager.CreateAsync(newUser, tempPass);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, roleName);

                    // Generar el token de confirmación de correo electrónico
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                    var tokenEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                    // Construir la URL de confirmación
                    var request = httpContext.Request;
                    var confirmationLink = $"{request.Scheme}://{request.Host}/Account/ConfirmEmail?userId={newUser.Id}&code={WebUtility.UrlEncode(tokenEncoded)}";

                    //detalles del correo
                    string emailSubject = "Confirmación de creación de usuario - SGRH";
                    string email = newUser.Email;
                    string username = newUser.FullName;
                    string message = $"Estimable {username},<br><br>" +
                        $"Se ha creado una cuenta para usted en nuestro sistema. " +
                        $"Para completar el proceso de registro, por favor haga clic en el siguiente enlace:<br><br>" +
                        $"<a href=\"{confirmationLink}\">Presione aquí</a><br><br>" +
                        $"Su clave temporal es: <strong> {tempPass} </strong><br><br>" +
                        $"Si usted no ha solicitado este registro, por favor ignore este mensaje.<br><br>" +
                        $"Gracias.<br><br> **Por favor no responda a este correo electrónico.**";

                    await _emailService.SendEmail(emailSubject, email, username, message, isHtml: true);

                    return (true, "Usuario creado exitosamente, se le ha notificado via correo electronico.");

                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        if (error.Code == "DuplicateUserName")
                        {
                            return (false, "El coreo electrónico se encuentra en uso. Por favor, elija otro.");
                        }

                    }

                    return (false, "Ocurrio un error durante la creación del usuario.");
                }
            }
            catch (Exception ex)
            {
                return (false, "Ocurrio un error al crear el usuario");
            }

        }


        public async Task<bool> UpdateUser(UserViewModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.Id);

                if (user == null)
                {
                    return false;
                }

                var workPeriod = await _context.WorkPeriod.FindAsync(model.WorkPeriodId);
                if (workPeriod == null)
                {
                    return (false);
                }

                user.Name = model.Name;
                user.LastName = model.LastName;
                user.Dni = model.Dni;
                user.BirthDate = model.BirthDate;
                user.PhoneNumber = model.PhoneNumber;
                user.Email = model.Email;
                if (user.DepartmentId != model.DepartmentId)
                {
                    user.DepartmentId = model.DepartmentId;
                }

                if (user.PositionId != model.PositionId)
                {
                    user.PositionId = model.PositionId;
                }

                if (user.UserType != model.UserType)
                {
                    user.UserType = model.UserType ?? throw new ArgumentNullException(nameof(model.UserType), "El UserType no puede ser nulo.");
                }
                user.HiredDate = model.HiredDate;
                user.BaseSalary = model.BaseSalary ?? throw new ArgumentNullException(nameof(model.BaseSalary), "El salario base no puede ser nulo.");
                user.workPeriod = workPeriod;
                user.IsActive = model.IsActive;


                string roleName = Enum.GetName(typeof(UserType), model.UserType);
                bool roleExists = await _roleManager.RoleExistsAsync(roleName);

                if (!roleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }

                var currentRoles = await _userManager.GetRolesAsync(user);

                var newRole = Enum.GetName(typeof(UserType), model.UserType);

                if (!currentRoles.Contains(newRole))
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles.ToArray());

                    await _userManager.AddToRoleAsync(user, newRole);
                }

                var result = await _userManager.UpdateAsync(user);

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public async Task<List<User>> GetAllUsersWithDetails()
        {
            return await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Position)
                .Include(u => u.workPeriod)
                .ToListAsync();
        }

        public async Task<List<Department>> GetDepartments()
        {
            return await _context.Departments.ToListAsync();
        }

        public async Task<List<Position>> GetPositions()
        {
            return await _context.Positions.ToListAsync();
        }

        public async Task<List<WorkPeriod>> GetWorkPeriods()
        {
            return await _context.WorkPeriod.ToListAsync();
        }


        public async Task<List<Position>> GetPositionsByDepartment(int departmentId)
        {
            return await _context.Positions
                .Where(p => p.DepartmentId == departmentId)
                .ToListAsync();
        }

        public async Task<(bool success, string errorMessage)> DeleteUser(UserViewModel model, string currentUserId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return (false, "No se encontró el usuario indicado.");
                }

                if (user.Id == currentUserId)
                {
                    return (false, "No puedes eliminar tu propio usuario.");
                }

                // Verificar si el usuario actual es Administrador
                var currentUser = await _userManager.FindByIdAsync(currentUserId);
                if (currentUser.UserType != UserType.Administrador)
                {
                    return (false, "Solo los usuarios de tipo Administrador pueden eliminar usuarios.");
                }

                // Verificar si el usuario a eliminar es un Administrador y el usuario actual no lo es
                if (user.UserType == UserType.Administrador && currentUser.UserType != UserType.Administrador)
                {
                    return (false, "Solo los Administradores pueden eliminar usuarios Administradores.");
                }


                var hasAbsences = _context.Absences.Any(a => a.UserId == user.Id);
                var hasAttendance = _context.Attendances.Any(a => a.UserId == user.Id);
                var hasDossier = _context.Dossiers.Any(d => d.User.Id == user.Id);
                var hasPersonalActionsCreatedByUser = _context.PersonalActions.Any(pa => pa.User.Id == user.Id);
                var hasPersonalActionsApprovedByUser = _context.PersonalActions.Any(pa => pa.ApprovedByUser.Id == user.Id);
                var hasPayrolls = _context.Payrolls.Any(p => p.User.Id == user.Id);
                var hasWarnings = _context.Warnings.Any(W => W.User.Id == user.Id);



                if (hasWarnings)
                {
                    var warnings = _context.Warnings.Where(w => w.User.Id == user.Id);
                    _context.Warnings.RemoveRange(warnings);
                }

                if (hasAbsences)
                {
                    var absences = _context.Absences.Include(d => d.Document).Where(a => a.UserId == user.Id);
                    foreach (var absence in absences)
                    {
                        _context.Document.RemoveRange(absence.Document);
                    }
                    _context.Absences.RemoveRange(absences);
                }

                if (hasAttendance)
                {
                    var attendances = _context.Attendances.Where(a => a.UserId == user.Id);
                    _context.Attendances.RemoveRange(attendances);
                }

                if (hasDossier)
                {
                    var dossiers = _context.Dossiers.Include(d => d.Document).Where(d => d.User.Id == user.Id);
                    foreach (var dossier in dossiers)
                    {
                        _context.Document.RemoveRange(dossier.Document);
                    }
                    _context.Dossiers.RemoveRange(dossiers);
                }

                if (hasPersonalActionsCreatedByUser)
                {
                    var personalActionsCreatedByUser = _context.PersonalActions.Where(pa => pa.User.Id == user.Id);
                    _context.PersonalActions.RemoveRange(personalActionsCreatedByUser);

                    var vacations = _context.Vacations.Where(v => v.PersonalAction.User.Id == user.Id);
                    _context.Vacations.RemoveRange(vacations);

                    var overtimes = _context.Overtimes.Where(o => o.PersonalAction.User.Id == user.Id);
                    _context.Overtimes.RemoveRange(overtimes);

                    var warnings = _context.Warnings.Where(w => w.PersonalAction.User.Id == user.Id);
                    _context.Warnings.RemoveRange(warnings);

                    var layoffs = _context.Layoffs.Where(s => s.PersonalAction.User.Id == user.Id);
                    _context.Layoffs.RemoveRange(layoffs);

                    var settlements = _context.Settlements.Where(w => w.Layoff.PersonalAction.User.Id == user.Id);
                    _context.Settlements.RemoveRange(settlements);

                }
                
                

                if (hasPersonalActionsApprovedByUser)
                {
                    var personalActionsApprovedByUser = _context.PersonalActions.Where(pa => pa.ApprovedByUser.Id == user.Id);
                    foreach (var action in personalActionsApprovedByUser)
                    {
                        action.ApprovedByUser = null;
                    }
                }


                if (hasPayrolls)
                {
                    var payrolls = _context.Payrolls.Where(p => p.User.Id == user.Id);
                    _context.Payrolls.RemoveRange(payrolls);
                }

                await _context.SaveChangesAsync();
                var result = await _userManager.DeleteAsync(user);
                return (result.Succeeded, null);
            }
            catch (Exception ex)
            {
                return (false, "Error al eliminar el usuario.");
            }
        }



        public async Task<UserViewModel> GetCurrentUserProfile()
        {
            var userId = _userManager.GetUserId(_signInManager.Context.User);
            return await GetUserViewModelById(userId);
        }


        public async Task<string> GetUserData(string searchTerm)
        {
            var users = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Position)
                .Include(u => u.workPeriod)
                .Where(u => u.Dni.StartsWith(searchTerm) || u.Name.StartsWith(searchTerm))
                .ToListAsync();

            if (users == null || users.Count == 0)
            {
                throw new Exception("User not found");
            }

            var userData = users.Select(user => new
            {
                Id = user.Id,
                FullName = user.FullName,
                Dni = user.Dni,
                BirthDate = user.BirthDate,
                Department = user.Department != null ? user.Department.Department_Name : "N/A",
                Position = user.Position != null ? user.Position.Position_Name : "N/A",
                UserType = Enum.GetName(typeof(UserType), user.UserType),
                Age = user.Age,
                BaseSalary = user.BaseSalary,
                PeriodName = user.workPeriod.PeriodName,
                VacationDays = user.VacationDays
            }).ToList();

            return JsonConvert.SerializeObject(userData);
        }

        public async Task<User> GetUserById(string userId)
        {
            return await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Position)
                .Include(u => u.workPeriod)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User> FindByDniAsync(string dni)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Dni == dni);
        }

        public async Task<bool> SetStatus(string id, bool status)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            user.IsActive = status;

            var result = await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            return result.Succeeded;
        }

        public async Task<int> GetEmployeeCountByDepartment(string userId)
        {
            var sup = await _userManager.FindByIdAsync(userId);

            var count = await _context.Users
                .Where(o => o.DepartmentId == sup.DepartmentId)
                .CountAsync();

            return count;
        }

        public async Task<int> GetEmployeeCount()
        {
            var count = await _context.Users
                .CountAsync();

            return count;
        }


        /*
        public async Task<IdentityResult> UpdateUser(User user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task AssignRole(User user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task<IdentityResult> ChangePassword(User user, string oldPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        }

        async Task IServiceUser.SignOut()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> CreateUser(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<SignInResult> LogIn(LoginViewModel model)
        {
            return await _signInManager.PasswordSignInAsync(
                model.Username,
                model.Password,
                model.RememberMe,
                false);
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(X => X.Email == email); 
        }

        public async Task<User> GetUserById(string id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id.ToString());
        }

        public async Task<bool> UserInRole(User user, string NameRole)
        {
            return await _userManager.IsInRoleAsync(user, NameRole);
        }

        public async Task VerifyRole(string nameRole)
        {
            bool roleExists = await _roleManager.RoleExistsAsync(nameRole);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Name = nameRole
                });
            }
        }

        public async Task UpdateRoles(User user, List<string> roleNames)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.Except(roleNames);
            var rolesToAdd = roleNames.Except(currentRoles);

            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            await _userManager.AddToRolesAsync(user, rolesToAdd);
        }

        async Task IServiceUser.RemoveRole(User user, string roleName)
        {
            await _userManager.RemoveFromRoleAsync(user, roleName);
        }

        public async Task<List<User>> GetUsersByDepartment(int departmentId)
        {
            return await _context.Users
                .Where(u => u.Department.Id_Department == departmentId)
                .ToListAsync();
        }

        public async Task<List<User>> GetUsersByPosition(int positionId)
        {
            return await _context.Users
                .Where(u => u.Position.Id_Position == positionId)
                .ToListAsync();
        }

        public async Task<IdentityResult> DeleteUser(User user)
        {
            return await _userManager.DeleteAsync(user);
        }

        public async Task<IdentityResult> ActivateUser(User user)
        {
            user.LockoutEnd = null;
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> DeactivateUser(User user)
        {
            user.LockoutEnd = DateTime.UtcNow.AddYears(100);
            return await _userManager.UpdateAsync(user);
        }

        */

    }
}
