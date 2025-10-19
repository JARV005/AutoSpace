using System.ComponentModel.DataAnnotations;

namespace AutoSpace.DTOs
{
    public class TicketDto
    {
        [Required]
        [StringLength(50)]
        public string TicketNumber { get; set; } = string.Empty;

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public int OperatorId { get; set; }
    }

    public class TicketExitDto
    {
        [Required]
        public int TicketId { get; set; }

        [Required]
        public int OperatorId { get; set; }
    }

    public class TicketResponseDto
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalMinutes { get; set; }
        public string QRCode { get; set; } = string.Empty;
        public string? SubscriptionStatus { get; set; }
    }
}