using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSpace.Models
{
    public class Rate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string TypeVehicle { get; set; } = "Car";

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal HourPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? AddPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MaxPrice { get; set; }

        public int? GraceTime { get; set; } // en minutos

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}