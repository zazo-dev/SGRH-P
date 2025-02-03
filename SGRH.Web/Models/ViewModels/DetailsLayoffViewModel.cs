using SGRH.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace SGRH.Web.Models.ViewModels
{
    public class DetailsLayoffViewModel
    {
        public int LayoffId { get; set; }

        public string userId { get; set; }

        [Display(Name = "Tipo de Acción")]
        public PersonalAction PersonalAction { get; set; }

        [Display(Name = "Responsabilidad Patronal")]
        [Required(ErrorMessage = "Favor indicar la responsabilidad patronal.")]
        public bool HasEmployerResponsibility { get; set; }

        [Display(Name = "Fecha de Despido")]
        [DataType(DataType.Date)]
        public DateTime DismissalDate { get; set; }

        [Display(Name = "Causa del Despido")]
        [Required(ErrorMessage = "Favor indicar la causa del despido.")]
        public string DismissalCause { get; set; }

        [Display(Name = "Hecho por")]
        public string RegisteredBy { get; set; }

        [Display(Name = "Liquidación Procesada")]
        public bool HasProcessed { get; set; }
    }
}
