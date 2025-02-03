using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SGRH.Web.Enums;

namespace SGRH.Web.Models
{
    public class CreatePayrollViewModel
    {
        public int Id_Payroll { get; set; }

        [Required(ErrorMessage = "El campo UserId es requerido.")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Los datos del empleado son requeridos, favor realice la búsqueda.")]
        [Display(Name = "Nombre del Empleado")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Por favor, seleccione un periodo de pago")]
        [Display(Name = "Periodo de pago")]
        public int? PayrollPeriodId { get; set; }

        public IEnumerable<PayrollPeriodViewModel> PayrollPeriods { get; set; }

        [Required(ErrorMessage = "Por favor, seleccione una frecuencia de nómina")]
        [Display(Name = "Frecuencia de Nómina")]
        public PayrollFrequency? PayrollFrequency { get; set; }

        [Display(Name = "Salario ordinario")]
        public decimal OrdinarySalary { get; set; }

        [Display(Name = "Cantidad de Horas Extra")]
        public decimal OtHours { get; set; }

        [Display(Name = "Monto total por Horas Extra")]
        public decimal OtHoursAmount { get; set; }

        [Display(Name = "Banco Popular")]
        public decimal BancoPopular { get; set; }

        [Display(Name = "CCSS S.E.M.")]
        public decimal EnfermedadMaternidad { get; set; }

        [Display(Name = "CCSS I.V.M.")]
        public decimal IVM { get; set; }

        [Display(Name = "Monto Total Deducciones")]
        public decimal TotalDeductions { get; set; }

        [Display(Name = "Salario Bruto")]
        public decimal GrossSalary { get; set; }

        [Display(Name = "Salario Neto")]
        public decimal NetSalary { get; set; }
    }

    public class PayrollPeriodViewModel
    {
        public int Id { get; set; }

        public string PeriodName { get; set; }
    }
}