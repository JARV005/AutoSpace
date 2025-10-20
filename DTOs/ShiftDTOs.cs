namespace AutoSpace.DTOs
{
    public class CreateShiftDto
    {
        public int OperatorId { get; set; }
        public decimal? InitialCash { get; set; }
    }

    public class ShiftDto
    {
        public int Id { get; set; }
        public int OperatorId { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal? InitialCash { get; set; }
        public decimal? FinalCash { get; set; }
        public decimal? TotalCashPayments { get; set; }
        public decimal? TotalCardPayments { get; set; }
        public decimal? TotalCollected => (TotalCashPayments ?? 0) + (TotalCardPayments ?? 0);
        public string Status => EndTime.HasValue ? "Closed" : "Active";
    }

    public class CloseShiftDto
    {
        public int ShiftId { get; set; }
        public decimal? FinalCash { get; set; }
        public decimal? TotalCashPayments { get; set; }
        public decimal? TotalCardPayments { get; set; }
    }
}