namespace AutoSpace.DTOs
{
    public class CreateUserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Document { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Document { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int VehicleCount { get; set; }
        public int SubscriptionCount { get; set; }
    }

    public class UpdateUserDto
    {
        public string? FullName { get; set; }
        public string? Document { get; set; }
        public string? Email { get; set; }
        public string? Status { get; set; }
    }
}