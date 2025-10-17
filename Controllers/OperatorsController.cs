using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSpace.Data;
using AutoSpace.Models;

namespace AutoSpace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OperatorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OperatorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Operator>>> GetOperators()
        {
            return await _context.Operators
                .Where(o => o.IsActive)
                .OrderBy(o => o.FullName)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Operator>> GetOperator(int id)
        {
            var operatorObj = await _context.Operators.FindAsync(id);

            if (operatorObj == null)
            {
                return NotFound();
            }

            return operatorObj;
        }

        [HttpPost]
        public async Task<ActionResult<Operator>> CreateOperator(Operator operatorObj)
        {
            _context.Operators.Add(operatorObj);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOperator), new { id = operatorObj.Id }, operatorObj);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOperator(int id, Operator operatorObj)
        {
            if (id != operatorObj.Id)
            {
                return BadRequest();
            }

            _context.Entry(operatorObj).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OperatorExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOperator(int id)
        {
            var operatorObj = await _context.Operators.FindAsync(id);
            if (operatorObj == null)
            {
                return NotFound();
            }

            operatorObj.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OperatorExists(int id)
        {
            return _context.Operators.Any(e => e.Id == id);
        }
    }
}