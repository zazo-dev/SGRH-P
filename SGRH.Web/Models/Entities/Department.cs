using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SGRH.Web.Models.Entities
{
    public class Department
    {
        [Key]
        public int Id_Department { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(100,ErrorMessage ="El campo {0} debe tener máximo {1} caractéres.")]
        [Display(Name = "Departamento")]
        public string Department_Name { get; set; }
    }
}
