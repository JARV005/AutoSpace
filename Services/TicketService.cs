using Microsoft.EntityFrameworkCore;
using AutoSpace.Data;
using AutoSpace.Models;
using AutoSpace.DTOs;

namespace AutoSpace.Services
{
    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TicketService> _logger;

        public TicketService(ApplicationDbContext context, ILogger<TicketService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Ticket> CreateEntryAsync(TicketDto ticketDto)
        {
            // Check if there's already an active ticket for this vehicle
            var activeTicket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.VehicleId == ticketDto.VehicleId && t.ExitTime == null);

            if (activeTicket != null)
            {
                throw new InvalidOperationException("Ya existe un ticket activo para este vehículo");
            }

            // Check for active subscription for this vehicle
            var activeSubscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.VehicleId == ticketDto.VehicleId && 
                                         s.Status == "Active" && 
                                         s.StartDate <= DateTime.UtcNow && 
                                         s.EndDate >= DateTime.UtcNow);

            // Get the vehicle to know the type and then get the rate for that type
            var vehicle = await _context.Vehicles
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == ticketDto.VehicleId);

            if (vehicle == null)
            {
                throw new InvalidOperationException("Vehículo no encontrado");
            }

            var rate = await _context.Rates
                .FirstOrDefaultAsync(r => r.TypeVehicle == vehicle.Type);

            if (rate == null)
            {
                throw new InvalidOperationException($"No hay tarifa configurada para el tipo de vehículo: {vehicle.Type}");
            }

            var ticket = new Ticket
            {
                TicketNumber = ticketDto.TicketNumber,
                VehicleId = ticketDto.VehicleId,
                OperatorId = ticketDto.OperatorId,
                SubscriptionId = activeSubscription?.Id,
                RateId = rate.Id,
                EntryTime = DateTime.UtcNow,
                QRCode = GenerateQRCode(ticketDto.TicketNumber, vehicle.Plate)
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Ticket {TicketId} created for vehicle {VehicleId}", ticket.Id, ticket.VehicleId);
            return ticket;
        }

        public async Task<Ticket> ProcessExitAsync(TicketExitDto exitDto)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Vehicle)
                .Include(t => t.Operator)
                .Include(t => t.Subscription)
                .Include(t => t.Rate)
                .FirstOrDefaultAsync(t => t.Id == exitDto.TicketId && t.ExitTime == null);

            if (ticket == null)
            {
                throw new KeyNotFoundException("Ticket no encontrado o ya cerrado");
            }

            ticket.ExitTime = DateTime.UtcNow;
            var duration = ticket.ExitTime.Value - ticket.EntryTime;
            ticket.TotalMinutes = (int)duration.TotalMinutes;

            // If it's a subscription, no payment needed
            if (ticket.SubscriptionId != null && ticket.Subscription.Status == "Active")
            {
                ticket.TotalAmount = 0;
            }
            else
            {
                ticket.TotalAmount = CalculateAmount(ticket.EntryTime, ticket.ExitTime.Value, ticket.Rate);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Ticket {TicketId} closed for vehicle {VehicleId}, Amount: {Amount}", ticket.Id, ticket.VehicleId, ticket.TotalAmount);

            return ticket;
        }

        public decimal CalculateAmount(DateTime entryTime, DateTime exitTime, Rate rate)
        {
            var duration = exitTime - entryTime;
            var totalMinutes = (decimal)duration.TotalMinutes;

            // Parse grace time (assuming it's in minutes)
            if (!int.TryParse(rate.GraceTime, out int graceMinutes))
            {
                graceMinutes = 30; // default
            }

            // Subtract grace period
            var chargeableMinutes = Math.Max(0, totalMinutes - graceMinutes);

            if (chargeableMinutes <= 0) return 0;

            // Calculate hours (round up to nearest hour)
            var chargeableHours = Math.Ceiling(chargeableMinutes / 60);

            var amount = rate.HourPrice;
            
            // Add additional rate for extra hours
            if (chargeableHours > 1)
            {
                amount += (chargeableHours - 1) * rate.AddPrice;
            }

            // Apply daily cap if configured
            if (rate.MaxPrice > 0 && amount > rate.MaxPrice)
            {
                amount = rate.MaxPrice;
            }

            return amount;
        }

        private string GenerateQRCode(string ticketNumber, string plate)
        {
            // Generate QR code string as per the requirement
            return $"TICKET:{ticketNumber}|PLATE:{plate}|DATE:{(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds}";
        }
    }
}