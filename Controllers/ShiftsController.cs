using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSpace.Data;
using AutoSpace.Models;

namespace AutoSpace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShiftsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ShiftsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Shift>>> GetShifts()
        {
            return await _context.Shifts
                .Include(s => s.Operator)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Shift>> CreateShift(Shift shift)
        {
            shift.StartTime = DateTime.UtcNow;
            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetShifts), new { id = shift.Id }, shift);
        }

        [HttpPut("{id}/close")]
        public async Task<IActionResult> CloseShift(int id, Shift shiftUpdate)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null)
            {
                return NotFound();
            }

            shift.EndTime = DateTime.UtcNow;
            shift.FinalCash = shiftUpdate.FinalCash;
            shift.TotalCashPayments = shiftUpdate.TotalCashPayments;
            shift.TotalCardPayments = shiftUpdate.TotalCardPayments;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}