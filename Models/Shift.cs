using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AutoSpace.Models;
public class Shift
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey("Operator")]
    public int OperatorId { get; set; }
    
    public DateTime StartTime { get; set; }
    
    public DateTime? EndTime { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal InitialCash { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? FinalCash { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCashPayments { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCardPayments { get; set; }
    
    // Navigation properties
    public virtual Operator Operator { get; set; }
}