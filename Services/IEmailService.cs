using AutoSpace.Models;

namespace AutoSpace.Services
{
    public interface IEmailService
    {
        Task SendSubscriptionConfirmation(Subscription subscription);
        Task SendSubscriptionExpirationWarning(Subscription subscription);
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}