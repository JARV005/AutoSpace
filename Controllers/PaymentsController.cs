using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSpace.Data;
using AutoSpace.Models;
using AutoSpace.DTOs;

namespace AutoSpace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPayments([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var query = _context.Payments
                .Include(p => p.Operator)
                .Include(p => p.Subscription)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(p => p.PaymentTime >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(p => p.PaymentTime <= toDate.Value);
            }

            var payments = await query
                .OrderByDescending(p => p.PaymentTime)
                .ToListAsync();

            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Operator)
                .Include(p => p.Subscription)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                return NotFound();
            }

            return payment;
        }

        [HttpPost]
        public async Task<ActionResult<Payment>> CreatePayment(Payment payment)
        {
            payment.PaymentTime = DateTime.UtcNow;

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
        }

        [HttpGet("today")]
        public async Task<ActionResult<object>> GetTodayPayments()
        {
            var today = DateTime.UtcNow.Date;
            
            var payments = await _context.Payments
                .Where(p => p.PaymentTime.Date == today)
                .Include(p => p.Operator)
                .ToListAsync();

            var total = payments.Sum(p => p.Amount);
            var cashTotal = payments.Where(p => p.PaymentMethod == "Cash").Sum(p => p.Amount);
            var cardTotal = payments.Where(p => p.PaymentMethod == "Card").Sum(p => p.Amount);

            return Ok(new
            {
                Total = total,
                CashTotal = cashTotal,
                CardTotal = cardTotal,
                Count = payments.Count,
                Payments = payments
            });
        }
    }
}