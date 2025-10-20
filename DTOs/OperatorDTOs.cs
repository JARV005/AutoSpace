namespace AutoSpace.DTOs
{
    public class CreateOperatorDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Document { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
    }

    public class OperatorDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Document { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TicketsProcessed { get; set; }
        public decimal TotalCollected { get; set; }
    }

    public class UpdateOperatorDto
    {
        public string? FullName { get; set; }
        public string? Document { get; set; }
        public string? Email { get; set; }
        public string? Status { get; set; }
        public bool? IsActive { get; set; }
    }
}