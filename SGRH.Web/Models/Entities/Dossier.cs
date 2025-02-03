using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SGRH.Web.Enums;

namespace SGRH.Web.Models.Entities
{
    public class Dossier
    {
        [Key]
        public int Id_Record { get; set; }

        public User User { get; set; }

        [Display(Name = "Tipo de documento")]
        public DocumentType DocumentType { get; set; }

        [MaxLength(500, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Descripción Adicional")]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha")]
        public DateTime? Date { get; set; }

        public virtual ICollection<Document> Document { get; set; }
    }
}