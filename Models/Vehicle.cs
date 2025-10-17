using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AutoSpace.Models;
public class Vehicle
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Plate { get; set; }
    
    [MaxLength(50)]
    public string Type { get; set; }
    
    [ForeignKey("User")]
    public int UserId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; }
    public virtual ICollection<Subscription> Subscriptions { get; set; }
    public virtual ICollection<Ticket> Tickets { get; set; }
}