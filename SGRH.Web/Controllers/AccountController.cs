using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using SGRH.Web.Models;
using SGRH.Web.Models.Entities;
using SGRH.Web.Services;
using SGRH.Web.ViewModels;
using SGRH.Web.Views.Account;
using System.Text;
using System.Threading.Tasks;

namespace SGRH.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IServiceUser _serviceUser;
        private readonly EmailService _emailService;


        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager, IServiceUser serviceUser, EmailService emailService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _serviceUser = serviceUser;
            _emailService = emailService;
        }

        public IActionResult Login()
        {
            return View("_Login");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, [FromQuery] string returnUrl = null)
        {
            ViewBag.NombreUbicacion = "Iniciar Sesión";
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.UserName);
                if (user != null)
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError(string.Empty, "Debe confirmar su correo electrónico antes de iniciar sesión.");
                        return View("_Login", model);
                    }

                    var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);
                    if (result.Succeeded)
                    {
                        // Usuario inicia sesión correctamente, restablece el contador de intentos fallidos
                        await _userManager.ResetAccessFailedCountAsync(user);
                        // Elimina la fecha de finalización del bloqueo
                        await _userManager.SetLockoutEndDateAsync(user, null);

                        if (!string.IsNullOrEmpty(returnUrl))
                        {
                            return LocalRedirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Home");
                    }
                    if (result.IsLockedOut)
                    {
                        // Usuario bloqueado
                        var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                        var currentTime = DateTimeOffset.Now;
                        if (lockoutEnd > currentTime)
                        {
                            // Usuario sigue bloqueado, puedes mostrar un mensaje al usuario indicando cuánto tiempo falta para desbloquearlo
                            var remainingTime = (int)Math.Ceiling((lockoutEnd.Value - currentTime).TotalMinutes);
                            ModelState.AddModelError(string.Empty, $"El usuario está bloqueado. Por favor, inténtalo de nuevo en {remainingTime} minutos.");
                        }
                        else
                        {
                            // Resetear el contador de intentos fallidos cuando no haya bloqueo
                            await _userManager.ResetAccessFailedCountAsync(user);
                            // El tiempo de bloqueo ha pasado, puedes desbloquear al usuario
                            await _userManager.SetLockoutEndDateAsync(user, null); // Desbloquea al usuario
                            ModelState.AddModelError(string.Empty, "El usuario ha sido desbloqueado. Por favor, intenta iniciar sesión nuevamente.");
                        }
                    }
                    else
                    {
                        var accessFailedCount = await _userManager.GetAccessFailedCountAsync(user); // Obtiene el contador actual de intentos fallidos
                        if (accessFailedCount >= 3)
                        {
                            // Si el usuario ha excedido el número máximo de intentos fallidos, lo bloqueamos
                            var lockoutEnd = DateTimeOffset.Now.AddMinutes(15); // Establece la hora de desbloqueo a 15 minutos después de la hora actual
                            await _userManager.SetLockoutEndDateAsync(user, lockoutEnd); // Establece la hora de desbloqueo del usuario
                            ModelState.AddModelError(string.Empty, "La cuenta está bloqueada debido a demasiados intentos fallidos de inicio de sesión. Por favor, intente en otro momento.");
                            // Resetear el contador de intentos fallidos cuando no haya bloqueo
                            await _userManager.ResetAccessFailedCountAsync(user);
                        }
                        else
                        {
                            // Si el usuario no ha excedido el número máximo de intentos fallidos, mostramos un mensaje de error genérico
                            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrecta. Comprueba que has introducido tu correo electrónico y contraseña correctamente.");
                            return View("_Login", model);
                        }
                    }
                }
            }
            return View("_Login", model);
        }



        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View("_ForgotPassword");
        }

        [HttpGet]
        public IActionResult ResetPassword(string username)
        {
            ViewBag.NombreUbicacion = "Resetear Contraseña";
            var viewModel = new ResetPasswordViewModel { UserName = username };
            return View("_ResetPassword",viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("_ResetPassword", model);
            }

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Usuario no encontrado");
                return View("_ResetPassword", model);
            }

            // Verifica que la contraseña temporal sea correcta
            var isTempPasswordValid = await _userManager.CheckPasswordAsync(user, model.TemporaryPassword);
            if (!isTempPasswordValid)
            {
                ModelState.AddModelError(string.Empty, "La contraseña temporal no es válida");
                return View("_ResetPassword", model);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded)
            {
                // Almacena un mensaje de éxito en TempData
                TempData["ResetPasswordSuccessMessage"] = "La contraseña se ha cambiado satisfactoriamente, favor validar las nuevas credenciales.";

                // Redirige a la acción Login con un indicador de éxito
                return RedirectToAction("Login", new { success = true });
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("_ResetPassword", model);
            }
        }

        public IActionResult PasswordResetSuccess()
        {
            var model = new PasswordResetSuccessViewModel();
            return View(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction("Index", "Home"); 
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"No se pudo encontrar al usuario con ID '{userId}'.");
            }

            var tokenBytes = WebEncoders.Base64UrlDecode(code);
            var tokenDecoded = Encoding.UTF8.GetString(tokenBytes);
            var result = await _userManager.ConfirmEmailAsync(user, tokenDecoded);
            if (result.Succeeded)
            {

                // Almacena un mensaje de éxito en TempData
                TempData["ConfirmEmailSuccessMessage"] = "Gracias por confirmar tu dirección email, por favor cambia tu clave temporal.";

                return RedirectToAction("ResetPassword", new { username = user.UserName });

            }
            else
            {
                return RedirectToAction("Error"); 
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendTemporaryPassword(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                TempData["ErrorMessage"] = "Por favor, ingrese un correo electrónico.";
                return View("_ForgotPassword");
            }

            var user = await _userManager.FindByEmailAsync(userName);

            if (user == null)
            {
                TempData["ErrorMessage"] = "No se encontró un usuario con el correo electrónico proporcionado.";
                return RedirectToAction("Login");
            }

            if (user.isTempPasswordUsed)
            {
                TempData["ErrorMessage"] = "Ya se ha generado una contraseña temporal para este usuario.";
                return RedirectToAction("ResetPassword");
            }

            string tempPass = _serviceUser.GenerateTempPass();

            user.TempPassword = tempPass;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, tempPass);

            if (!result.Succeeded)
            {
                // Manejar el error de actualización de usuario
                return RedirectToAction("Error");
            }

            user.isTempPasswordUsed = true;

            await _userManager.UpdateAsync(user);

            string emailSubject = "Nueva contraseña temporal";
            string email = user.Email;
            string username = user.FullName;
            string resetUrl = Url.Action("ResetPassword", "Account", new { userName = user.Email, code = token }, protocol: HttpContext.Request.Scheme);
            string message = $"Estimado {username}, su nueva contraseña temporal es: <strong>{tempPass}</strong><br/><br/> Para restablecer su contraseña, haga clic en el siguiente enlace: <a href='{resetUrl}'>Restablecer Contraseña</a>.<br/><br/>**Por favor no responda a este correo electrónico.**";

            await _emailService.SendEmail(emailSubject, email, username, message, isHtml: true);

            TempData["SuccessMessageNotification"] = "Se ha enviado una nueva contraseña temporal por correo electrónico, favor validar."; 
            return RedirectToAction("ForgotPassword");
        }


        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

    }

}
