using Microsoft.EntityFrameworkCore;
using AutoSpace.Data;
using AutoSpace.Models;
using AutoSpace.DTOs;

namespace AutoSpace.Services
{
    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _context;

        public TicketService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Ticket> RegisterEntryAsync(CreateTicketDto createTicketDto)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == createTicketDto.VehicleId);
            if (vehicle == null)
                throw new ArgumentException("El vehículo especificado no existe");

            var activeTicket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.VehicleId == createTicketDto.VehicleId && t.ExitTime == null);
            if (activeTicket != null)
                throw new InvalidOperationException("El vehículo ya tiene un ticket activo");

            var activeSubscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.VehicleId == createTicketDto.VehicleId && 
                                         s.Status == "Active" && 
                                         s.EndDate > DateTime.UtcNow);

            var rate = await _context.Rates
                .FirstOrDefaultAsync(r => r.TypeVehicle == vehicle.Type && r.IsActive);

            var ticket = new Ticket
            {
                TicketNumber = GenerateTicketNumber(),
                VehicleId = createTicketDto.VehicleId,
                OperatorId = createTicketDto.OperatorId,
                SubscriptionId = activeSubscription?.Id,
                RateId = rate?.Id,
                EntryTime = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            await _context.Entry(ticket).Reference(t => t.Vehicle).LoadAsync();
            await _context.Entry(ticket).Reference(t => t.Operator).LoadAsync();

            return ticket;
        }

        public async Task<Ticket> RegisterExitAsync(ExitTicketDto exitTicketDto)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Vehicle)
                .Include(t => t.Subscription)
                .Include(t => t.Rate)
                .FirstOrDefaultAsync(t => t.Id == exitTicketDto.TicketId);

            if (ticket == null)
                throw new ArgumentException("Ticket not found");

            if (ticket.ExitTime.HasValue)
                throw new InvalidOperationException("Ticket already closed");

            ticket.ExitTime = DateTime.UtcNow;
            ticket.OperatorId = exitTicketDto.OperatorId;

            var totalMinutes = (int)(ticket.ExitTime.Value - ticket.EntryTime).TotalMinutes;
            ticket.TotalMinutes = totalMinutes;

            if (ticket.SubscriptionId.HasValue && 
                ticket.Subscription?.Status == "Active" && 
                ticket.Subscription.EndDate > DateTime.UtcNow)
            {
                ticket.TotalAmount = 0;
            }
            else
            {
                ticket.TotalAmount = await CalculateAmountAsync(
                    ticket.EntryTime, 
                    ticket.ExitTime.Value, 
                    ticket.Vehicle.Type);
            }

            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<decimal> CalculateAmountAsync(DateTime entryTime, DateTime exitTime, string vehicleType)
        {
            var rate = await _context.Rates
                .FirstOrDefaultAsync(r => r.TypeVehicle == vehicleType && r.IsActive);

            if (rate == null)
                throw new InvalidOperationException($"No rate found for vehicle type: {vehicleType}");

            var duration = exitTime - entryTime;
            
            // 🛠️ CORRECCIÓN APLICADA
            double totalHoursDouble = Math.Ceiling(duration.TotalHours);
            decimal totalHours = (decimal)totalHoursDouble;

            decimal calculatedAmount = totalHours * rate.HourPrice;

            if (rate.MaxPrice.HasValue && calculatedAmount > rate.MaxPrice.Value)
                return rate.MaxPrice.Value;

            return calculatedAmount;
        }

        private string GenerateTicketNumber()
        {
            return "TKT" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        }
    }
}