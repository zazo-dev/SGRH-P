using SGRH.Web.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGRH.Web.Models.Entities
{
    public class Overtime
    {
        [Key]
        public int Id_OT { get; set; }

        public PersonalAction PersonalAction { get; set; }

        [Display(Name = "Jornada")]
        public string WorkPeriodName { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha")]
        public DateTime OT_Date { get; set; }

        [Display(Name = "Cantidad de horas")]
        public int Hours_Worked {  get; set; }

        [Display(Name = "Tipo de Hora Extra")]
        public TypeOT TypeOT { get; set; }

        [Display(Name = "Monto calculado de horas extra")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal AmountOT { get; set; }
    }
}
