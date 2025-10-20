namespace AutoSpace.DTOs
{
    public class CreateRateDto
    {
        public string TypeVehicle { get; set; } = "Car";
        public decimal HourPrice { get; set; }
        public decimal? AddPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? GraceTime { get; set; }
    }

    public class RateDto
    {
        public int Id { get; set; }
        public string TypeVehicle { get; set; } = string.Empty;
        public decimal HourPrice { get; set; }
        public decimal? AddPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? GraceTime { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateRateDto
    {
        public string? TypeVehicle { get; set; }
        public decimal? HourPrice { get; set; }
        public decimal? AddPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? GraceTime { get; set; }
        public bool? IsActive { get; set; }
    }
}