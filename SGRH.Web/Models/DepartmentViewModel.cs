using System.ComponentModel.DataAnnotations;

namespace SGRH.Web.Models
{
    public class DepartmentViewModel
    {
        public int DepartmentId { get; set; }

        [Display(Name ="Departamento")]
        public string DepartmentName { get; set; }
    }
}
