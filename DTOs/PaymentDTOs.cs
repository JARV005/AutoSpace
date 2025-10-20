namespace AutoSpace.DTOs
{
    public class CreatePaymentDto
    {
        public int? TicketId { get; set; }
        public int? SubscriptionId { get; set; }
        public int? OperatorId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string? ReferenceNumber { get; set; }
    }

    public class PaymentDto
    {
        public int Id { get; set; }
        public int? TicketId { get; set; }
        public int? SubscriptionId { get; set; }
        public int? OperatorId { get; set; }
        public string? OperatorName { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentTime { get; set; }
        public string? ReferenceNumber { get; set; }
    }
}