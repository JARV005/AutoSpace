// DTOs/ReportDTOs.cs
public class IncomeReportDto
{
    public decimal TotalIncome { get; set; }
    public int TotalVehicles { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<DailyIncomeDto> DailyIncomes { get; set; }
}

public class DailyIncomeDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public int VehicleCount { get; set; }
}

public class SubscriptionReportDto
{
    public int TotalSubscriptions { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int ExpiredSubscriptions { get; set; }
    public decimal TotalRevenue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<SubscriptionDetailDto> Subscriptions { get; set; }
}

public class SubscriptionDetailDto
{
    public int Id { get; set; }
    public string VehiclePlate { get; set; }
    public string VehicleType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal MonthlyPrice { get; set; }
    public string Status { get; set; }
    public string UserName { get; set; }
}

public class UserReportDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<UserPerformanceDto> UserPerformances { get; set; }
}

public class UserPerformanceDto
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Document { get; set; }
    public string Status { get; set; }
    public int TotalSubscriptions { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class OperatorReportDto
{
    public int TotalOperators { get; set; }
    public int ActiveOperators { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<OperatorPerformanceDto> OperatorPerformances { get; set; }
}

public class OperatorPerformanceDto
{
    public int OperatorId { get; set; }
    public string OperatorName { get; set; }
    public string Email { get; set; }
    public string Document { get; set; }
    public string Status { get; set; }
    public bool IsActive { get; set; }
    public int TotalTickets { get; set; }
    public int TotalPayments { get; set; }
    public int TotalShifts { get; set; }
}

public class VehicleReportDto
{
    public int TotalVehicles { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<VehicleStatsDto> VehicleStats { get; set; }
}

public class VehicleStatsDto
{
    public int VehicleId { get; set; }
    public string Plate { get; set; }
    public string Type { get; set; }
    public int TotalSubscriptions { get; set; }
    public decimal TotalRevenue { get; set; }
}