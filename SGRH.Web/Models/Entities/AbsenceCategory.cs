using System.ComponentModel.DataAnnotations;

namespace SGRH.Web.Models.Entities
{
    public class AbsenceCategory
    {
        [Key]
        public int Id_Absence_Category { get; set; }

        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Display(Name = "Nombre de Categoria")]
        [Required]
        public string Category_Name { get; set; }
    }
}
