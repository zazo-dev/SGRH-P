using System.ComponentModel.DataAnnotations;

namespace SGRH.Web.Models
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "El campo para el correo electrónico es obligatorio.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "El campo para la nueva contraseña es obligatorio.")]
        [StringLength(100, ErrorMessage = "El campo {0} debe tener como mínimo {2} y como máximo {1} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "El campo para confirmar la nueva contraseña es obligatorio.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Nueva Contraseña")]
        [Compare("NewPassword", ErrorMessage = "La nueva contraseña y la contraseña de confirmación no coinciden.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "El campo para la contraseña temporal es obligatorio.")]
        [Display(Name = "Contraseña Temporal")]
        public string TemporaryPassword { get; set; }
    }
}
