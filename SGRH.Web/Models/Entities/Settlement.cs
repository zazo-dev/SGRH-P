using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGRH.Web.Models.Entities
{
    public class Settlement
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "ID de Despido")]
        public int LayoffId { get; set; }

        [ForeignKey("LayoffId")]
        public Layoff Layoff { get; set; }

        [Display(Name = "Promedio mensual de los últimos 6 meses")]
        public decimal AvgLast6MonthsSalary { get; set; }

        [Display(Name = "Promedio diario de los últimos 6 meses")]
        public decimal DailyAvgLast6Months { get; set; }

        [Display(Name = "Aguinaldo")]
        public decimal Bonus { get; set; }

        [Display(Name = "Saldo de Vacaciones no gozadas")]
        public int UnenjoyedVacation { get; set; }

        [Display(Name = "Monto por Vacaciones no gozadas")]
        public decimal UnenjoyedVacationAmount { get; set; }

        [Display(Name = "Cantidad de días de Preaviso")]
        public int Notice { get; set; }

        [Display(Name = "Monto por Preaviso")]
        public decimal NoticeAmount { get; set; }

        [Display(Name = "Cantidad de días de Cesantía")]
        public int Severance { get; set; }

        [Display(Name = "Monto por Cesantía")]
        public decimal SeveranceAmount { get; set; }

        [Display(Name = "Total de Liquidación")]
        public decimal TotalSettlement { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha de Liquidación")]
        public DateTime SettlementDate { get; set; }

    }
}
