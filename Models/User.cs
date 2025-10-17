using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace AutoSpace.Models;
public class User
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string FullName { get; set; }
    
    [MaxLength(50)]
    public string Document { get; set; }
    
    [MaxLength(255)]
    public string Email { get; set; }
    
    [MaxLength(50)]
    public string Status { get; set; }
    
    // Navigation properties
    public virtual ICollection<Vehicle> Vehicles { get; set; }
    public virtual ICollection<Subscription> Subscriptions { get; set; }
    public virtual ICollection<Mail> Mails { get; set; }
}