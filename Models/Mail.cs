using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSpace.Models
{
    public class Mail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public int? UserId { get; set; }

        public int? SubscriptionId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        
        [ForeignKey("SubscriptionId")]
        public virtual Subscription? Subscription { get; set; }
    }
}