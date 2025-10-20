namespace AutoSpace.DTOs
{
    public class CreateVehicleDto
    {
        public string Plate { get; set; } = string.Empty;
        public string Type { get; set; } = "Car";
        public int UserId { get; set; }
    }

    public class VehicleDto
    {
        public int Id { get; set; }
        public string Plate { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int TicketCount { get; set; }
        public bool HasActiveSubscription { get; set; }
    }

    public class UpdateVehicleDto
    {
        public string? Plate { get; set; }
        public string? Type { get; set; }
        public int? UserId { get; set; }
    }
}