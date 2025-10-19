using System.ComponentModel.DataAnnotations;

namespace AutoSpace.DTOs
{
    public class SubscriptionDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal MonthlyPrice { get; set; }
    }

    public class SubscriptionResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public string VehiclePlate { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal MonthlyPrice { get; set; }
        public string SubscriptionStatus { get; set; } = string.Empty;
    }
}