using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AutoSpace.Models;
public class Ticket
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string TicketNumber { get; set; }
    
    [ForeignKey("Vehicle")]
    public int VehicleId { get; set; }
    
    [ForeignKey("Operator")]
    public int OperatorId { get; set; }
    
    [ForeignKey("Subscription")]
    public int? SubscriptionId { get; set; }
    
    [ForeignKey("Rate")]
    public int RateId { get; set; }
    
    public DateTime EntryTime { get; set; }
    
    public DateTime? ExitTime { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    
    public int TotalMinutes { get; set; }
    
    [MaxLength(500)]
    public string QRCode { get; set; }
    
    // Navigation properties
    public virtual Vehicle Vehicle { get; set; }
    public virtual Operator Operator { get; set; }
    public virtual Subscription Subscription { get; set; }
    public virtual Rate Rate { get; set; }
}