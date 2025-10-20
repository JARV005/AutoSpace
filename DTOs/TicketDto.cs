namespace AutoSpace.DTOs
{
    public class CreateTicketDto
    {
        public string TicketNumber { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public int? OperatorId { get; set; }
        public int? SubscriptionId { get; set; }
        public int? RateId { get; set; }
    }

    public class TicketDto
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public string VehiclePlate { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public int? OperatorId { get; set; }
        public string? OperatorName { get; set; }
        public int? SubscriptionId { get; set; }
        public int? RateId { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public decimal? TotalAmount { get; set; }
        public int? TotalMinutes { get; set; }
        public string? QRCode { get; set; }
        public TimeSpan? Duration { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ExitTicketDto
    {
        public int TicketId { get; set; }
        public int OperatorId { get; set; }
    }
}