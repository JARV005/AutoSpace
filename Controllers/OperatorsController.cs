using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSpace.Data;
using AutoSpace.Models;
using AutoSpace.DTOs;

namespace AutoSpace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OperatorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OperatorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OperatorDto>>> GetOperators()
        {
            var operators = await _context.Operators
                .Include(o => o.Tickets)
                .Include(o => o.Payments)
                .Include(o => o.Shifts)
                .Select(o => new OperatorDto
                {
                    Id = o.Id,
                    FullName = o.FullName,
                    Document = o.Document,
                    Email = o.Email,
                    Status = o.Status,
                    IsActive = o.IsActive,
                    CreatedAt = o.CreatedAt,
                    TicketsProcessed = o.Tickets.Count,
                    TotalCollected = o.Payments.Where(p => p.TicketId != null).Sum(p => p.Amount)
                })
                .ToListAsync();

            return operators;
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<OperatorDto>>> GetActiveOperators()
        {
            var operators = await _context.Operators
                .Where(o => o.IsActive)
                .Include(o => o.Tickets)
                .Include(o => o.Payments)
                .Include(o => o.Shifts)
                .Select(o => new OperatorDto
                {
                    Id = o.Id,
                    FullName = o.FullName,
                    Document = o.Document,
                    Email = o.Email,
                    Status = o.Status,
                    IsActive = o.IsActive,
                    CreatedAt = o.CreatedAt,
                    TicketsProcessed = o.Tickets.Count,
                    TotalCollected = o.Payments.Where(p => p.TicketId != null).Sum(p => p.Amount)
                })
                .ToListAsync();

            return operators;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OperatorDto>> GetOperator(int id)
        {
            var operatorEntity = await _context.Operators
                .Include(o => o.Tickets)
                .Include(o => o.Payments)
                .Include(o => o.Shifts)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (operatorEntity == null)
            {
                return NotFound();
            }

            var operatorDto = new OperatorDto
            {
                Id = operatorEntity.Id,
                FullName = operatorEntity.FullName,
                Document = operatorEntity.Document,
                Email = operatorEntity.Email,
                Status = operatorEntity.Status,
                IsActive = operatorEntity.IsActive,
                CreatedAt = operatorEntity.CreatedAt,
                TicketsProcessed = operatorEntity.Tickets.Count,
                TotalCollected = operatorEntity.Payments.Where(p => p.TicketId != null).Sum(p => p.Amount)
            };

            return operatorDto;
        }

        [HttpPost]
        public async Task<ActionResult<OperatorDto>> CreateOperator(CreateOperatorDto createOperatorDto)
        {
            // Verificar si el documento o email ya existen
            var existingOperator = await _context.Operators
                .FirstOrDefaultAsync(o => o.Document == createOperatorDto.Document || o.Email == createOperatorDto.Email);
            if (existingOperator != null)
            {
                return BadRequest(new { error = "Ya existe un operador con el mismo documento o email" });
            }

            var operatorEntity = new Operator
            {
                FullName = createOperatorDto.FullName,
                Document = createOperatorDto.Document,
                Email = createOperatorDto.Email,
                Status = createOperatorDto.Status,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Operators.Add(operatorEntity);
            await _context.SaveChangesAsync();

            var operatorDto = new OperatorDto
            {
                Id = operatorEntity.Id,
                FullName = operatorEntity.FullName,
                Document = operatorEntity.Document,
                Email = operatorEntity.Email,
                Status = operatorEntity.Status,
                IsActive = operatorEntity.IsActive,
                CreatedAt = operatorEntity.CreatedAt,
                TicketsProcessed = 0,
                TotalCollected = 0
            };

            return CreatedAtAction(nameof(GetOperator), new { id = operatorEntity.Id }, operatorDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOperator(int id, UpdateOperatorDto updateOperatorDto)
        {
            var operatorEntity = await _context.Operators.FindAsync(id);
            if (operatorEntity == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(updateOperatorDto.FullName))
                operatorEntity.FullName = updateOperatorDto.FullName;

            if (!string.IsNullOrEmpty(updateOperatorDto.Document))
                operatorEntity.Document = updateOperatorDto.Document;

            if (!string.IsNullOrEmpty(updateOperatorDto.Email))
                operatorEntity.Email = updateOperatorDto.Email;

            if (!string.IsNullOrEmpty(updateOperatorDto.Status))
                operatorEntity.Status = updateOperatorDto.Status;

            if (updateOperatorDto.IsActive.HasValue)
                operatorEntity.IsActive = updateOperatorDto.IsActive.Value;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOperator(int id)
        {
            var operatorEntity = await _context.Operators.FindAsync(id);
            if (operatorEntity == null)
            {
                return NotFound();
            }

            _context.Operators.Remove(operatorEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}