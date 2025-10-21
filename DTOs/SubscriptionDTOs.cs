namespace AutoSpace.DTOs
{
    public class CreateSubscriptionDto
    {
        public int UserId { get; set; }
        public int VehicleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyPrice { get; set; }
    }

    public class SubscriptionDTOs
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public int VehicleId { get; set; }
        public string VehiclePlate { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsExpired => EndDate < DateTime.UtcNow;
        public int DaysUntilExpiry => (EndDate - DateTime.UtcNow).Days;
    }

    public class UpdateSubscriptionDto
    {
        public DateTime? EndDate { get; set; }
        public decimal? MonthlyPrice { get; set; }
        public string? Status { get; set; }
    }
}