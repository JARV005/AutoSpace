using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSpace.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public int? TicketId { get; set; }

        public int? SubscriptionId { get; set; }

        public int? OperatorId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public string PaymentMethod { get; set; } = "Cash"; // Cash, Card

        public DateTime PaymentTime { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string? ReferenceNumber { get; set; }

        // Navigation properties
        [ForeignKey("TicketId")]
        public virtual Ticket? Ticket { get; set; }
        
        [ForeignKey("SubscriptionId")]
        public virtual Subscription? Subscription { get; set; }
        
        [ForeignKey("OperatorId")]
        public virtual Operator? Operator { get; set; }
    }
}