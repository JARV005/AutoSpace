using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AutoSpace.Models;
public class Subscription
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey("User")]
    public int UserId { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    [MaxLength(50)]
    public string Status { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal MonthlyPrice { get; set; }
    
    [ForeignKey("Vehicle")]
    public int VehicleId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; }
    public virtual Vehicle Vehicle { get; set; }
    public virtual ICollection<Payment> Payments { get; set; }
    public virtual ICollection<Ticket> Tickets { get; set; }
    public virtual ICollection<Mail> Mails { get; set; }
}