using System.ComponentModel.DataAnnotations;

namespace SGRH.Web.Models.Entities
{
    public class PayrollPeriod
    {
        [Key]
        public int Id { get; set; } 

        [Display(Name = "Periodo de pago")]
        public string PeriodName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha Inicio")]
        public DateTime StartDate { get; set; } 

        [DataType(DataType.Date)]
        [Display(Name = "Fecha final")]
        public DateTime EndDate { get; set; }
    }
}
