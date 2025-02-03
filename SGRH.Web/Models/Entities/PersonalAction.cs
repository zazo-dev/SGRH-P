using SGRH.Web.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGRH.Web.Models.Entities
{
    public class PersonalAction
    {
        [Key]
        public int Id_Action { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Display(Name ="Tipo de Acción")]
        public ActionType ActionType { get; set; }

        [DataType(DataType.MultilineText)]
        [MaxLength(200, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Display(Name = "Descripción")]
        public String Description { get; set; }

        [Display(Name = "Fecha")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public User User { get; set; }

        public bool? Is_Approved { get; set; }

        [ForeignKey("Approved_By_User")]
        public int? Approved_By_User { get;}
        
        [Display(Name = "Aprobado por")]
        public User ApprovedByUser { get; set; }

        [Display(Name = "Fecha Aprobación")]
        public DateTime? Approval_Date { get; set; }

        [Display(Name = "Estado")]
        public Status? Status { get; set; }

    }
}
