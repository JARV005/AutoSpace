namespace AutoSpace.DTOs
{
    public class DashboardMetricsDto
    {
        public int VehiclesInside { get; set; }
        public decimal TodayIncome { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int ExpiringSubscriptions { get; set; }
        public int TodayTickets { get; set; }
    }

    public class WeeklyIncomeDto
    {
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
    }
}