using System;
using System.ComponentModel.DataAnnotations;

namespace SGRH.Web.Models.Entities
{
    public class Layoff
    {
        [Key]
        public int Id { get; set; }

        public PersonalAction PersonalAction { get; set; }

        [Display(Name = "Responsabilidad Patronal")]
        public bool HasEmployerResponsibility { get; set; }

        [Display(Name = "Fecha de Despido")]
        [DataType(DataType.Date)]
        public DateTime DismissalDate { get; set; }

        [Display(Name = "Causa del Despido")]
        public string DismissalCause { get; set; }

        [Display(Name = "Hecho por")]
        public string RegisteredBy { get; set; }

        [Display(Name = "Liquidación Procesada")]
        public bool HasProcessed { get; set; }
    }
}
