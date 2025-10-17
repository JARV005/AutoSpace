using System.Net;
using System.Net.Mail;
using AutoSpace.Data;
using Microsoft.Extensions.Options;
using AutoSpace.Models;

namespace AutoSpace.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly ApplicationDbContext _context;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger, ApplicationDbContext context)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _context = context;
        }

        public async Task SendSubscriptionConfirmation(Subscription subscription)
        {
            var user = await _context.Users.FindAsync(subscription.UserId);
            if (user == null || string.IsNullOrEmpty(user.Email)) return;

            var vehicle = await _context.Vehicles.FindAsync(subscription.VehicleId);
            var subject = "Confirmación de Mensualidad - AutoSpace";
            var body = $@"
                <h2>Estimado {user.FullName},</h2>
                <p>Su mensualidad para el vehículo con placa <strong>{vehicle?.Plate}</strong> ha sido creada exitosamente.</p>
                <p><strong>Período:</strong> {subscription.StartDate:dd/MM/yyyy} - {subscription.EndDate:dd/MM/yyyy}</p>
                <p><strong>Precio Mensual:</strong> {subscription.MonthlyPrice:C}</p>
                <p>Gracias por preferirnos.</p>
                <br>
                <p><em>Equipo AutoSpace</em></p>";

            await SendEmailAsync(user.Email, subject, body);
        }

        public async Task SendSubscriptionExpirationWarning(Subscription subscription)
        {
            var user = await _context.Users.FindAsync(subscription.UserId);
            if (user == null || string.IsNullOrEmpty(user.Email)) return;

            var vehicle = await _context.Vehicles.FindAsync(subscription.VehicleId);
            var subject = "Recordatorio: Su mensualidad está por vencer";
            var body = $@"
                <h2>Estimado {user.FullName},</h2>
                <p>Le informamos que su mensualidad para el vehículo con placa <strong>{vehicle?.Plate}</strong> vencerá el {subscription.EndDate:dd/MM/yyyy}.</p>
                <p>Por favor, renueve su mensualidad para continuar disfrutando de nuestros servicios.</p>
                <br>
                <p><em>Equipo AutoSpace</em></p>";

            await SendEmailAsync(user.Email, subject, body);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                if (string.IsNullOrEmpty(toEmail)) return;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                
                mailMessage.To.Add(toEmail);

                using var smtpClient = new SmtpClient(_emailSettings.SmtpServer)
                {
                    Port = _emailSettings.Port,
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                    EnableSsl = true
                };

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", toEmail);
                // No throw - email failure shouldn't break the application
            }
        }
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int Port { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}