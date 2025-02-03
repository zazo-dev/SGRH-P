using System.ComponentModel.DataAnnotations;

namespace SGRH.Web.Models.Entities
{
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del archivo es obligatorio.")]
        [MaxLength(255, ErrorMessage = "El nombre del archivo no puede tener más de 255 caracteres.")]
        public string FileName { get; set; }

        [Required(ErrorMessage = "El contenido del archivo es obligatorio.")]
        public byte[] Content { get; set; }

        [Required(ErrorMessage = "El tipo MIME del archivo es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El tipo MIME del archivo no puede tener más de 100 caracteres.")]
        public string ContentType { get; set; }

        [Required(ErrorMessage = "El tamaño del archivo es obligatorio.")]
        public long Size { get; set; }

        [Required(ErrorMessage = "La fecha de creación del archivo es obligatoria.")]
        public DateTime CreatedAt { get; set; }

    }
}
