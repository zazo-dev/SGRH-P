using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using SGRH.Web.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGRH.Web.Models.Entities
{
    public class User : IdentityUser
    {

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Display(Name = "Apellidos")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(11, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Display(Name = "Cédula")]
        public string Dni { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime? BirthDate { get; set; }

         [Display(Name = "Departamento")]
        public int? DepartmentId { get; set; }
 
        [Display(Name = "Puesto")]
        public int? PositionId { get; set; }
 
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }
 
        [ForeignKey("PositionId")]
        public virtual Position Position { get; set; }

        [Display(Name = "Rol")]
        public UserType UserType { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem> Departments { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem> Positions { get; set; }

        [NotMapped]
        [Display(Name = "Edad")]
        public int Age
        {
            get
            {
                if (BirthDate.HasValue)
                {
                    DateTime today = DateTime.Today;
                    int age = today.Year - BirthDate.Value.Year;

                    // Restar un año si el cumpleaños aún no ha pasado este año
                    if (BirthDate > today.AddYears(-age))
                    {
                        age--;
                    }

                    return age;
                }

                return 0; // Otra valor por defecto si no hay fecha de nacimiento
            }
        }

        [Display(Name = "Saldo de Vacaciones")]
        public int VacationDays { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de contratación")]
        public DateTime? HiredDate { get; set; }

        [ForeignKey("WorkPeriodId")]
        public virtual WorkPeriod workPeriod { get; set; }

        [Display(Name = "Salario Base")]
        [DisplayFormat(DataFormatString = "₡ {0:N2}")]
        public decimal BaseSalary { get; set; }

        [NotMapped]
        [Display(Name = "Nombre Completo")]
        public String FullName => Name + " " + LastName;

        [NotMapped]
        public string TempPassword { get; set; }

        [NotMapped]
        public bool isTempPasswordUsed { get; set; }

        [Display(Name = "Activo")]
        public bool IsActive { get; set; }
    }
}
