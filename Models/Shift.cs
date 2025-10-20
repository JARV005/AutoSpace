using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSpace.Models
{
    public class Shift
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OperatorId { get; set; }

        [Required]
        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        public DateTime? EndTime { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? InitialCash { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? FinalCash { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? TotalCashPayments { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? TotalCardPayments { get; set; }

        // Navigation properties
        [ForeignKey("OperatorId")]
        public virtual Operator Operator { get; set; } = null!;
    }
}