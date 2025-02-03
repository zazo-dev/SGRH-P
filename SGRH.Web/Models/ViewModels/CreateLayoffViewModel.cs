using System;
using System.ComponentModel.DataAnnotations;
using SGRH.Web.Models.Entities;

namespace SGRH.Web.Models.ViewModels
{
    public class CreateLayoffViewModel
    {
        [Required(ErrorMessage = "No es posible procesar la acción sin el usuario a despedir.")]
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
        [DataType(DataType.MultilineText)]
        [StringLength(350, ErrorMessage = "La longitud máxima de la causa del despido es de 350 caracteres.")]
        public string DismissalCause { get; set; }

        [Display(Name = "Hecho por")]
        [StringLength(50, ErrorMessage = "La longitud máxima del campo 'Hecho por' es de 50 caracteres.")]
        public string RegisteredBy { get; set; }

        public string currentUserId { get; set; }
    }
}
