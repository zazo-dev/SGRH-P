using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SGRH.Web.Models.Entities
{
    public class Warning
    {
        [Key]
        public int Id_Warnings { get; set; }
        public User User { get; set; }

        public PersonalAction PersonalAction { get; set; }

        [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Razón")]
        public string Reason { get; set; }

        [MaxLength(250, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Observaciones")]
        public string Observations { get; set; }

    }
}
