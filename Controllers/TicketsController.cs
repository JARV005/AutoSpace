using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSpace.Data;
using AutoSpace.Models;
using AutoSpace.DTOs;
using AutoSpace.Services;

namespace AutoSpace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITicketService _ticketService;

        public TicketsController(ApplicationDbContext context, ITicketService ticketService)
        {
            _context = context;
            _ticketService = ticketService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketDTOs>>> GetTickets()
        {
            var tickets = await _context.Tickets
                .Include(t => t.Vehicle)
                .Include(t => t.Operator)
                .Include(t => t.Subscription)
                .Include(t => t.Rate)
                .Select(t => new TicketDTOs
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber,
                    VehicleId = t.VehicleId,
                    VehiclePlate = t.Vehicle.Plate,
                    VehicleType = t.Vehicle.Type,
                    OperatorId = t.OperatorId,
                    OperatorName = t.Operator != null ? t.Operator.FullName : null,
                    SubscriptionId = t.SubscriptionId,
                    RateId = t.RateId,
                    EntryTime = t.EntryTime,
                    ExitTime = t.ExitTime,
                    TotalAmount = t.TotalAmount,
                    TotalMinutes = t.TotalMinutes,
                    QRCode = t.QRCode,
                    Duration = t.ExitTime.HasValue ? t.ExitTime.Value - t.EntryTime : null,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return tickets;
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<TicketDTOs>>> GetActiveTickets()
        {
            var tickets = await _context.Tickets
                .Where(t => t.ExitTime == null)
                .Include(t => t.Vehicle)
                .Include(t => t.Operator)
                .Include(t => t.Subscription)
                .Include(t => t.Rate)
                .Select(t => new TicketDTOs
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber,
                    VehicleId = t.VehicleId,
                    VehiclePlate = t.Vehicle.Plate,
                    VehicleType = t.Vehicle.Type,
                    OperatorId = t.OperatorId,
                    OperatorName = t.Operator != null ? t.Operator.FullName : null,
                    SubscriptionId = t.SubscriptionId,
                    RateId = t.RateId,
                    EntryTime = t.EntryTime,
                    ExitTime = t.ExitTime,
                    TotalAmount = t.TotalAmount,
                    TotalMinutes = t.TotalMinutes,
                    QRCode = t.QRCode,
                    Duration = DateTime.UtcNow - t.EntryTime,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return tickets;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TicketDTOs>> GetTicket(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Vehicle)
                .Include(t => t.Operator)
                .Include(t => t.Subscription)
                .Include(t => t.Rate)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            var ticketDto = new TicketDTOs
            {
                Id = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                VehicleId = ticket.VehicleId,
                VehiclePlate = ticket.Vehicle.Plate,
                VehicleType = ticket.Vehicle.Type,
                OperatorId = ticket.OperatorId,
                OperatorName = ticket.Operator != null ? ticket.Operator.FullName : null,
                SubscriptionId = ticket.SubscriptionId,
                RateId = ticket.RateId,
                EntryTime = ticket.EntryTime,
                ExitTime = ticket.ExitTime,
                TotalAmount = ticket.TotalAmount,
                TotalMinutes = ticket.TotalMinutes,
                QRCode = ticket.QRCode,
                Duration = ticket.ExitTime.HasValue ? ticket.ExitTime.Value - ticket.EntryTime : null,
                CreatedAt = ticket.CreatedAt
            };

            return ticketDto;
        }

        [HttpGet("vehicle/{vehicleId}/active")]
        public async Task<ActionResult<TicketDTOs>> GetActiveTicketByVehicle(int vehicleId)
        {
            var ticket = await _context.Tickets
                .Where(t => t.VehicleId == vehicleId && t.ExitTime == null)
                .Include(t => t.Vehicle)
                .Include(t => t.Operator)
                .Include(t => t.Subscription)
                .Include(t => t.Rate)
                .FirstOrDefaultAsync();

            if (ticket == null)
            {
                return NotFound();
            }

            var ticketDto = new TicketDTOs
            {
                Id = ticket.Id,
                TicketNumber = ticket.TicketNumber,
                VehicleId = ticket.VehicleId,
                VehiclePlate = ticket.Vehicle.Plate,
                VehicleType = ticket.Vehicle.Type,
                OperatorId = ticket.OperatorId,
                OperatorName = ticket.Operator != null ? ticket.Operator.FullName : null,
                SubscriptionId = ticket.SubscriptionId,
                RateId = ticket.RateId,
                EntryTime = ticket.EntryTime,
                ExitTime = ticket.ExitTime,
                TotalAmount = ticket.TotalAmount,
                TotalMinutes = ticket.TotalMinutes,
                QRCode = ticket.QRCode,
                Duration = DateTime.UtcNow - ticket.EntryTime,
                CreatedAt = ticket.CreatedAt
            };

            return ticketDto;
        }

        [HttpPost("entry")]
        public async Task<ActionResult<TicketDTOs>> RegisterEntry(CreateTicketDto createTicketDto)
        {
            try
            {
                var ticket = await _ticketService.RegisterEntryAsync(createTicketDto);
                
                var ticketDto = new TicketDTOs
                {
                    Id = ticket.Id,
                    TicketNumber = ticket.TicketNumber,
                    VehicleId = ticket.VehicleId,
                    VehiclePlate = ticket.Vehicle.Plate,
                    VehicleType = ticket.Vehicle.Type,
                    OperatorId = ticket.OperatorId,
                    OperatorName = ticket.Operator?.FullName,
                    SubscriptionId = ticket.SubscriptionId,
                    RateId = ticket.RateId,
                    EntryTime = ticket.EntryTime,
                    TotalAmount = ticket.TotalAmount,
                    TotalMinutes = ticket.TotalMinutes,
                    QRCode = ticket.QRCode,
                    CreatedAt = ticket.CreatedAt
                };

                return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticketDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("exit")]
        public async Task<ActionResult<TicketDTOs>> RegisterExit(ExitTicketDto exitTicketDto)
        {
            try
            {
                var ticket = await _ticketService.RegisterExitAsync(exitTicketDto);
                
                var ticketDto = new TicketDTOs
                {
                    Id = ticket.Id,
                    TicketNumber = ticket.TicketNumber,
                    VehicleId = ticket.VehicleId,
                    VehiclePlate = ticket.Vehicle.Plate,
                    VehicleType = ticket.Vehicle.Type,
                    OperatorId = ticket.OperatorId,
                    OperatorName = ticket.Operator?.FullName,
                    SubscriptionId = ticket.SubscriptionId,
                    RateId = ticket.RateId,
                    EntryTime = ticket.EntryTime,
                    ExitTime = ticket.ExitTime,
                    TotalAmount = ticket.TotalAmount,
                    TotalMinutes = ticket.TotalMinutes,
                    QRCode = ticket.QRCode,
                    Duration = ticket.ExitTime.HasValue ? ticket.ExitTime.Value - ticket.EntryTime : null,
                    CreatedAt = ticket.CreatedAt
                };

                return Ok(ticketDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}