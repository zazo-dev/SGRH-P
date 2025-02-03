using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGRH.Web.Models.Entities
{
    public class Vacation
    {
        [Key]
        public int Id_Vacation { get; set; }

        [Display(Name = "Tipo de Acción")]
        public PersonalAction PersonalAction { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Sale del")]
        public DateTime Start_Date { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Hasta el")]
        public DateTime End_Date { get; set;}

        [DataType(DataType.MultilineText)]
        [MaxLength(200, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Display(Name = "Comentarios adicionales")]
        public string Comments {  get; set; }

        [Display(Name = "Días Solicitados")]
        public int RequestedDays
        {
            get
            {
                return (End_Date - Start_Date).Days + 1;
            }
            set { }
        }
    }
}
