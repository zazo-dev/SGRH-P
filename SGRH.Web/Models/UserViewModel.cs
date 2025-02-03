using Microsoft.AspNetCore.Mvc.Rendering;
using SGRH.Web.Enums;
using SGRH.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace SGRH.Web.Models
{
    public class UserViewModel
    {

        public string Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Display(Name = "Nombre")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "El nombre solo debe contener letras.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Display(Name = "Apellidos")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Los apellidos solo deben contener letras.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [MaxLength(11, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Display(Name = "Cédula")]
        public string Dni { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime? BirthDate { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [Phone]
        [Display(Name = "Número telefónico")]
        [MaxLength(9, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [DataType(DataType.EmailAddress)]
        [MaxLength(40, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Display(Name ="Correo Electrónico")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [Display(Name = "Departamento")]
        public int? DepartmentId { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [Display(Name = "Puesto")]
        public int? PositionId { get; set; }

        public virtual DepartmentViewModel Department { get; set; }

        public virtual PositionViewModel Position { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [Display(Name = "Rol")]
        public UserType? UserType { get; set; }

        public IEnumerable<SelectListItem> Departments { get; set; }

        public IEnumerable<SelectListItem> Positions { get; set; }

        [Display(Name = "Saldo de Vacaciones")]
        public int VacationDays { get; set; }
        

        public List<SelectListItem> Roles { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Contratación")]
        public DateTime? HiredDate { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [Display(Name = "Salario Base")]
        [Range(0, double.MaxValue, ErrorMessage = "El salario base no debe ser negativo.")]
        [DisplayFormat(DataFormatString = "₡ {0:N2}")]
        [DataType(DataType.Currency)]
        public decimal? BaseSalary { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [Display(Name = "Jornada Laboral")]
        public int? WorkPeriodId { get; set; }
        public virtual WorkPeriod WorkPeriod { get; set; }

        public IEnumerable<SelectListItem> WorkPeriods { get; set; }

        public bool IsActive { get; set; }

    }
}
