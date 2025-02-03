using System.ComponentModel.DataAnnotations;

namespace SGRH.Web.Models
{
    public class PositionViewModel
    {
        public int PositionId { get; set; }
        [Display(Name = "Puesto")]
        public string PositionName { get; set; }
    }
}
