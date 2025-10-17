using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AutoSpace.Models;
public class Payment
{
    [Key]
    public int Id { get; set; }
    
    public int FeketId { get; set; }
    
    [ForeignKey("Subscription")]
    public int SubscriptionId { get; set; }
    
    [ForeignKey("Operator")]
    public int OperatorId { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    [MaxLength(50)]
    public string PaymentMethod { get; set; }
    
    public DateTime PaymentTime { get; set; }
    
    [MaxLength(100)]
    public string ReferenceNumber { get; set; }
    
    // Navigation properties
    public virtual Subscription Subscription { get; set; }
    public virtual Operator Operator { get; set; }
}