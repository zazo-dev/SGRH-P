using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.Identity.Client;
using SGRH.Web.Enums;

namespace SGRH.Web.Models.Entities
{
    public class Payroll
    {
        [Key]
        public int Id_Payroll { get; set; }

        public User User { get; set; }

        [Display(Name = "Periodo de pago")]
        public int PayrollPeriodId { get; set; }

        [ForeignKey("PayrollPeriodId")]
        public PayrollPeriod PayrollPeriod { get; set; }  

        [Display(Name = "Frecuencia de Nómina")]
        public PayrollFrequency PayrollFrequency { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Salario Ordinario")]
        public decimal OrdinarySalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cantidad de Horas Extra")]
        public decimal OtHours { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Monto total por Horas Extra")]
        public decimal OtHoursAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Banco Popular")]
        public decimal BancoPopular { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "CCSS S.E.M.")]
        public decimal EnfermedadMaternidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "CCSS I.V.M.")]
        public decimal IVM {  get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Monto Total Deducciones")]
        public decimal TotalDeductions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Salario Bruto")]
        public decimal GrossSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Salario Neto")]
        public decimal NetSalary { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha de Pago")]
        public DateTime PaymentDate { get; set; }

    }


}
