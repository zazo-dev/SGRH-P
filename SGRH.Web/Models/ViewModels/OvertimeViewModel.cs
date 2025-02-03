using SGRH.Web.Enums;
using System.ComponentModel.DataAnnotations;

namespace SGRH.Web.Models.ViewModels
{
    public class OvertimeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo Tipo de Acción es requerido.")]
        [Display(Name = "Tipo de Acción")]
        public int ActionType { get; set; }

        [Required(ErrorMessage = "El campo Descripción es requerido.")]
        [Display(Name = "Descripción")]
        [MaxLength(250, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "El campo Fecha es requerido.")]
        [Display(Name = "Fecha")]
        [DataType(DataType.Date)]
        public DateTime OT_Date { get; set; }

        [Display(Name = "Cantidad de Horas")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad de horas debe ser un número positivo y mayor que 0.")]
        [Required(ErrorMessage = "El campo Cantidad de Horas es requerido.")]
        public int Hours_Worked { get; set; }

        [Display(Name = "Tipo de Hora Extra")]
        [Required(ErrorMessage = "El campo Tipo de extras es requerido.")]
        public TypeOT TypeOT { get; set; }

        public int MaxOTHours { get; set; }
    }

}
