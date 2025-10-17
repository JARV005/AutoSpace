using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSpace.Data;
using AutoSpace.Models;
using AutoSpace.DTOs;
using AutoSpace.Services;

namespace AutoSpace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITicketService _ticketService;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(ApplicationDbContext context, ITicketService ticketService, ILogger<TicketsController> logger)
        {
            _context = context;
            _ticketService = ticketService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketResponseDto>>> GetTickets([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var query = _context.Tickets
                .Include(t => t.Vehicle)
                .Include(t => t.Operator)
                .Include(t => t.Subscription)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(t => t.EntryTime >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(t => t.EntryTime <= toDate.Value);
            }

            var tickets = await query
                .OrderByDescending(t => t.EntryTime)
                .Select(t => new TicketResponseDto
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber,
                    VehiclePlate = t.Vehicle.Plate,
                    OperatorName = t.Operator.FullName,
                    EntryTime = t.EntryTime,
                    ExitTime = t.ExitTime,
                    TotalAmount = t.TotalAmount,
                    TotalMinutes = t.TotalMinutes,
                    QRCode = t.QRCode,
                    SubscriptionStatus = t.Subscription != null ? t.Subscription.Status : null
                })
                .ToListAsync();

            return Ok(tickets);
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<TicketResponseDto>>> GetActiveTickets()
        {
            var tickets = await _context.Tickets
                .Where(t => t.ExitTime == null)
                .Include(t => t.Vehicle)
                .Include(t => t.Operator)
                .Select(t => new TicketResponseDto
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber,
                    VehiclePlate = t.Vehicle.Plate,
                    OperatorName = t.Operator.FullName,
                    EntryTime = t.EntryTime,
                    QRCode = t.QRCode
                })
                .OrderByDescending(t => t.EntryTime)
                .ToListAsync();

            return Ok(tickets);
        }

        [HttpPost("entry")]
        public async Task<ActionResult<Ticket>> CreateEntry(TicketDto ticketDto)
        {
            try
            {
                var ticket = await _ticketService.CreateEntryAsync(ticketDto);
                return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticket);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("exit")]
        public async Task<ActionResult<Ticket>> ProcessExit(TicketExitDto exitDto)
        {
            try
            {
                var ticket = await _ticketService.ProcessExitAsync(exitDto);
                return Ok(ticket);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Ticket>> GetTicket(int id)
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

            return ticket;
        }
    }
}