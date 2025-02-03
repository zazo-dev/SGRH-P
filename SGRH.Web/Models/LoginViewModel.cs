using System.ComponentModel.DataAnnotations;

namespace SGRH.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [StringLength(40, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres.")]
        [Display(Name = "Usuario")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string? Password { get; set; }

        [Display(Name = "Recuérdame")]
        public bool RememberMe { get; set; }
    }
}