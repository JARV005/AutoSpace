namespace AutoSpace.DTOs
{
    public class TicketDto
    {
        public string TicketNumber { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public int OperatorId { get; set; }
    }

    public class TicketExitDto
    {
        public int TicketId { get; set; }
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