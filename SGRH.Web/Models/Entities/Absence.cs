using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SGRH.Web.Enums;

namespace SGRH.Web.Models.Entities
{
    public class Absence
    {
        [Key]
        public int AbsenceId { get; set; }

        public User User { get; set; }

        public AbsenceCategory AbsenceCategory { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name ="Fecha de Inicio")]
        public DateTime? Start_Date { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha de Finalización")]
        public DateTime? End_Date { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Comentarios")]
        [MaxLength(500, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        public string Absence_Comments { get; set; }

        public virtual ICollection<Document> Document { get; set; }
        public string UserId { get; internal set; }
    }
}
