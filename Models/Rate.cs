using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AutoSpace.Models;
public class Rate
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(50)]
    public string TypeVehicle { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal HourPrice { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal AddPrice { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal MaxPrice { get; set; }
    
    [MaxLength(50)]
    public string GraceTime { get; set; }
    
    // Navigation properties
    public virtual ICollection<Ticket> Tickets { get; set; }
}