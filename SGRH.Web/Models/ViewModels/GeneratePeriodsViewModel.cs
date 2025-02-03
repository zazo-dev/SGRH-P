using System.ComponentModel.DataAnnotations;

namespace SGRH.Web.Models.ViewModels
{
    public class GeneratePeriodsViewModel
    {
        [Required(ErrorMessage = "El año es obligatorio.")]
        [RegularExpression(@"^20[2-9]\d$", ErrorMessage = "El año debe ser del 2020 en adelante.")]
        public int Year { get; set; }
    }
}
