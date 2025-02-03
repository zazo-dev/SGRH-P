using System.ComponentModel.DataAnnotations;

namespace SGRH.Web.Models
{
    public class AbsenceViewModel
    {
        [Required]
        public int Id { get; set; }

        [Display(Name = "Categoría de Ausencia")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Category { get; set; }

        [Display(Name = "Fecha de Inicio")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Fecha de Finalización")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Comentarios/Justificación")]
        [DataType(DataType.MultilineText)]
        [MaxLength(400, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        public string Comments { get; set; }

        [Display(Name = "Documentación (opcional)")]
        public List<IFormFile> Documentation { get; set; }
        public List<DocumentViewModel> Documentations { get; set; }

    }
}
