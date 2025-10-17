using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSpace.Models;
public class Mail
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(255)]
    public string Subject { get; set; }
    
    public string Body { get; set; }
    
    public DateTime SentAt { get; set; }
    
    [ForeignKey("User")]
    public int UserId { get; set; }
    
    [ForeignKey("Subscription")]
    public int SubscriptionId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; }
    public virtual Subscription Subscription { get; set; }
}