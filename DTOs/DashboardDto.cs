namespace AutoSpace.DTOs
{
    public class DashboardMetricsDto
    {
        public int TotalVehicles { get; set; }
        public int ActiveTickets { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int ExpiringSubscriptions { get; set; }
        public decimal DailyIncome { get; set; }
        public decimal WeeklyIncome { get; set; }
        public decimal MonthlyIncome { get; set; }
        public int ActiveOperators { get; set; }
    }

    public class WeeklyIncomeDto
    {
        public string Day { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public int TicketCount { get; set; }
    }

    public class CurrentVehicleDto
    {
        public int TicketId { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public DateTime EntryTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string? OperatorName { get; set; }
    }
}