using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSpace.Data;
using AutoSpace.DTOs;

namespace AutoSpace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("metrics")]
        public async Task<ActionResult<DashboardMetricsDto>> GetMetrics()
        {
            var today = DateTime.UtcNow.Date;
            
            var metrics = new DashboardMetricsDto
            {
                VehiclesInside = await _context.Tickets
                    .CountAsync(t => t.ExitTime == null),
                    
                TodayIncome = await _context.Payments
                    .Where(p => p.PaymentTime.Date == today)
                    .SumAsync(p => p.Amount),
                    
                ActiveSubscriptions = await _context.Subscriptions
                    .CountAsync(s => s.Status == "Active" && 
                                    s.StartDate <= DateTime.UtcNow && 
                                    s.EndDate >= DateTime.UtcNow),
                    
                ExpiringSubscriptions = await _context.Subscriptions
                    .CountAsync(s => s.Status == "Active" && 
                                    s.EndDate >= DateTime.UtcNow && 
                                    s.EndDate <= DateTime.UtcNow.AddDays(3)),
                    
                TodayTickets = await _context.Tickets
                    .CountAsync(t => t.EntryTime.Date == today)
            };

            return Ok(metrics);
        }

        [HttpGet("weekly-income")]
        public async Task<ActionResult<IEnumerable<WeeklyIncomeDto>>> GetWeeklyIncome()
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-7);
            
            var weeklyIncome = await _context.Payments
                .Where(p => p.PaymentTime >= startDate)
                .GroupBy(p => p.PaymentTime.Date)
                .Select(g => new WeeklyIncomeDto
                {
                    Date = g.Key,
                    Total = g.Sum(p => p.Amount)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return Ok(weeklyIncome);
        }

        [HttpGet("current-vehicles")]
        public async Task<ActionResult<IEnumerable<object>>> GetCurrentVehicles()
        {
            var vehicles = await _context.Tickets
                .Where(t => t.ExitTime == null)
                .Include(t => t.Vehicle)
                .Include(t => t.Operator)
                .Select(t => new
                {
                    t.Id,
                    t.TicketNumber,
                    VehiclePlate = t.Vehicle.Plate,
                    t.EntryTime,
                    OperatorName = t.Operator.FullName,
                    
                })
                .OrderByDescending(t => t.EntryTime)
                .ToListAsync();

            return Ok(vehicles);
        }
    }
}