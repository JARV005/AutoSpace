namespace AutoSpace.DTOs
{
    public class CreateMailDto
    {
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public int? SubscriptionId { get; set; }
    }

    public class MailDto
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public int? SubscriptionId { get; set; }
    }
}