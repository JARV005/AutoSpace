using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSpace.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TicketNumber { get; set; } = string.Empty;

        [Required]
        public int VehicleId { get; set; }

        public int? OperatorId { get; set; }

        public int? SubscriptionId { get; set; }

        public int? RateId { get; set; }

        [Required]
        public DateTime EntryTime { get; set; } = DateTime.UtcNow;

        public DateTime? ExitTime { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? TotalAmount { get; set; }

        public int? TotalMinutes { get; set; }

        public string? QRCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("VehicleId")]
        public virtual Vehicle Vehicle { get; set; } = null!;
        
        [ForeignKey("OperatorId")]
        public virtual Operator? Operator { get; set; }
        
        [ForeignKey("SubscriptionId")]
        public virtual Subscription? Subscription { get; set; }
        
        [ForeignKey("RateId")]
        public virtual Rate? Rate { get; set; }
        
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}