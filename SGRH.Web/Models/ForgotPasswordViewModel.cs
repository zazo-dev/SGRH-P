using System.ComponentModel.DataAnnotations;

namespace SGRH.Web.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "El campo para el correo electrónico es obligatorio.")]
        public string UserName { get; set; }
    }
}
