using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SGRH.Web.Models
{
    public class AddInitialVacationDaysViewModel
    {
        [Display(Name = "Buscar Usuario")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string SearchUser { get; set; }

        [Display(Name = "Días de Vacaciones")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public int Days { get; set; }
    }
}
