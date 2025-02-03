using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGRH.Web.Models.Entities
{
    public class Attendance
    {
        [Key]
        public int Id_Attendance { get; set; }

        public string UserId {  get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha")]
        public DateTime Date { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        [Display(Name = "Hora de Entrada")]
        public DateTime? EntryTime { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Hora de Salida")]
        public DateTime? ExitTime { get; set; }

        [NotMapped]
        public TimeSpan TotalTime
        {
            get
            {
                // Realizar el cálculo solo si ambos valores no son nulos
                if (ExitTime.HasValue && EntryTime.HasValue)
                {
                    return ExitTime.Value - EntryTime.Value;
                }
                else
                {
                    return TimeSpan.Zero;
                }
            }
        }

    }
}
