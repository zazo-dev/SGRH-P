using System.ComponentModel.DataAnnotations;

namespace SGRH.Web.Models.ViewModels
{
    public class VacationViewModel
    {
        [Required(ErrorMessage = "El campo Tipo de Acción es requerido.")]
        [Display(Name = "Tipo de Acción")]
        public int ActionType { get; set; }

        [Required(ErrorMessage = "El campo Descripción es requerido.")]
        [Display(Name = "Descripción")]
        [MaxLength(250, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "El campo Fecha de Salida es requerido.")]
        [Display(Name = "Sale del")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "El campo Fecha de Regreso es requerido.")]
        [Display(Name = "Hasta el")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [MaxLength(300, ErrorMessage = "El campo Comentarios adicionales debe tener máximo {1} caracteres.")]
        [Display(Name = "Comentarios adicionales")]
        public string Comments { get; set; }

        public int VacationBalance { get; set; }
    }
}
