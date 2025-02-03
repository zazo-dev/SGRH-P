using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGRH.Web.Models.Entities
{
    public class WorkPeriod
    {
        [Key] 
        public int Id { get; set; }

        [Required]
        [Display(Name = "Jornada")]
        public string PeriodName { get; set; }

        [Required]
        [Display(Name = "Descripción")]
        public string PeriodDescription { get; set; }

        [Required]
        [Display(Name = "Máxima cantidad de horas semanales")]
        public int MaxPeriodValue { get; set; }

        [Required]
        public int MaxDailyOTHours { get; set; }

    }
}
