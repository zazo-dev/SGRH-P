using SGRH.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace SGRH.Web.Models
{
    public class WarningViewModel : Warning
    {
        [Key]
        public int Id_Warnings { get; set; }

        public PersonalAction PersonalAction { get; set; }

        [MaxLength(150, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Razón")]
        public string Reason { get; set; }

        [MaxLength(350, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Observaciones")]
        public string Observations { get; set; }

    }
}
