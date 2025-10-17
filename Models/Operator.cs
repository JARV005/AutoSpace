using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace AutoSpace.Models;
public class Operator
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
    
    public bool IsActive { get; set; }
    
    // Navigation properties
    public virtual ICollection<Payment> Payments { get; set; }
    public virtual ICollection<Ticket> Tickets { get; set; }
    public virtual ICollection<Shift> Shifts { get; set; }
}