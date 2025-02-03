using SGRH.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace SGRH.Web.Models.ViewModels
{
    public class CreateSettlementViewModel
    {
        public int LayoffId { get; set; } 

        public Layoff Layoff { get; set; }

        //[Display(Name = "Salario de los últimos 6 meses")]
        //[Required(ErrorMessage = "Favor indicar el salario de los últimos 6 meses.")]
        //public decimal Last6MonthsSalary { get; set; }

        [Display(Name = "Promedio mensual de los últimos 6 meses")]
        public decimal AvgLast6MonthsSalary { get; set; }

        [Display(Name = "Promedio diario de los últimos 6 meses")]
        public decimal DailyAvgLast6Months { get; set; }

        [Display(Name = "Aguinaldo")]
        public decimal Bonus { get; set; }

        [Display(Name = "Saldo de Vacaciones no gozadas")]
        [Required(ErrorMessage = "Favor indicar la cantidad de vacaciones no gozadas.")]
        public int UnenjoyedVacation { get; set; }

        [Display(Name = "Monto por Vacaciones no gozadas")]
        public decimal UnenjoyedVacationAmount { get; set; }

        [Display(Name = "Cantidad de días de Preaviso")]
        [Required(ErrorMessage = "Favor indicar la cantidad de días de preaviso.")]
        public int Notice { get; set; }

        [Display(Name = "Monto por Preaviso")]
        public decimal NoticeAmount { get; set; }

        [Display(Name = "Cantidad de días de Cesantía")]
        [Required(ErrorMessage = "Favor indicar la cantidad de días de cesantía.")]
        public int Severance { get; set; }

        [Display(Name = "Monto por Cesantía")]
        public decimal SeveranceAmount { get; set; }

        [Display(Name = "Total de Liquidación")]
        public decimal TotalSettlement { get; set; }

        public bool ShowCalculatedlEntry { get; set; }

        public string currentUserId { get; set; }

        //// Constructor para inicializar la propiedad
        //public CreateSettlementViewModel()
        //{
        //    ShowCalculatedlEntry = false; // Por defecto, la opción de ingreso manual no está habilitada
        //}

    }
}
