using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSpace.Data;
using AutoSpace.DTOs;

namespace AutoSpace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            try
            {
                var totalVehicles = await _context.Vehicles.CountAsync();
                var activeTickets = await _context.Tickets.CountAsync(t => t.ExitTime == null);
                var activeSubscriptions = await _context.Subscriptions.CountAsync(s => s.Status == "Active" && s.EndDate > DateTime.UtcNow);
                
                var expiringSubscriptions = await _context.Subscriptions
                    .CountAsync(s => s.Status == "Active" && s.EndDate <= DateTime.UtcNow.AddDays(7) && s.EndDate > DateTime.UtcNow);

                var dailyIncome = await _context.Tickets
                    .Where(t => t.ExitTime.HasValue && t.ExitTime.Value.Date == DateTime.UtcNow.Date && t.TotalAmount.HasValue)
                    .SumAsync(t => t.TotalAmount.Value);

                var weeklyIncome = await _context.Tickets
                    .Where(t => t.ExitTime.HasValue && t.ExitTime.Value >= DateTime.UtcNow.AddDays(-7) && t.TotalAmount.HasValue)
                    .SumAsync(t => t.TotalAmount.Value);

                var monthlyIncome = await _context.Tickets
                    .Where(t => t.ExitTime.HasValue && t.ExitTime.Value >= DateTime.UtcNow.AddDays(-30) && t.TotalAmount.HasValue)
                    .SumAsync(t => t.TotalAmount.Value);

                var activeOperators = await _context.Operators.CountAsync(o => o.IsActive);

                var metrics = new DashboardMetricsDto
                {
                    TotalVehicles = totalVehicles,
                    ActiveTickets = activeTickets,
                    ActiveSubscriptions = activeSubscriptions,
                    ExpiringSubscriptions = expiringSubscriptions,
                    DailyIncome = dailyIncome,
                    WeeklyIncome = weeklyIncome,
                    MonthlyIncome = monthlyIncome,
                    ActiveOperators = activeOperators
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error retrieving dashboard metrics", details = ex.Message });
            }
        }

        [HttpGet("weekly-income")]
        public async Task<ActionResult<IEnumerable<WeeklyIncomeDto>>> GetWeeklyIncome()
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-6).Date;
                var endDate = DateTime.UtcNow.Date;

                var weeklyData = await _context.Tickets
                    .Where(t => t.ExitTime.HasValue && 
                               t.ExitTime.Value >= startDate && 
                               t.TotalAmount.HasValue)
                    .GroupBy(t => t.ExitTime.Value.Date)
                    .Select(g => new WeeklyIncomeDto
                    {
                        Day = g.Key.ToString("ddd"),
                        Income = g.Sum(t => t.TotalAmount.Value),
                        TicketCount = g.Count()
                    })
                    .ToListAsync();

                // Fill missing days
                var allDays = Enumerable.Range(0, 7)
                    .Select(offset => startDate.AddDays(offset))
                    .ToList();

                var result = allDays.Select(day =>
                {
                    var existing = weeklyData.FirstOrDefault(w => w.Day == day.ToString("ddd"));
                    return existing ?? new WeeklyIncomeDto
                    {
                        Day = day.ToString("ddd"),
                        Income = 0,
                        TicketCount = 0
                    };
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error retrieving weekly income data", details = ex.Message });
            }
        }

        [HttpGet("current-vehicles")]
        public async Task<ActionResult<IEnumerable<CurrentVehicleDto>>> GetCurrentVehicles()
        {
            try
            {
                var currentVehicles = await _context.Tickets
                    .Where(t => t.ExitTime == null)
                    .Include(t => t.Vehicle)
                    .Include(t => t.Operator)
                    .Select(t => new CurrentVehicleDto
                    {
                        TicketId = t.Id,
                        LicensePlate = t.Vehicle.Plate,
                        VehicleType = t.Vehicle.Type,
                        EntryTime = t.EntryTime,
                        Duration = DateTime.UtcNow - t.EntryTime,
                        OperatorName = t.Operator != null ? t.Operator.FullName : "System"
                    })
                    .ToListAsync();

                return Ok(currentVehicles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error retrieving current vehicles", details = ex.Message });
            }
        }

        // Nuevo endpoint para estadísticas adicionales
        [HttpGet("operator-stats")]
        public async Task<ActionResult> GetOperatorStats()
        {
            try
            {
                var operatorStats = await _context.Operators
                    .Where(o => o.IsActive)
                    .Select(o => new
                    {
                        OperatorId = o.Id,
                        OperatorName = o.FullName,
                        TicketsProcessed = o.Tickets.Count(t => t.ExitTime.HasValue),
                        TotalCollected = o.Payments.Where(p => p.TicketId != null).Sum(p => p.Amount),
                        ActiveShifts = o.Shifts.Count(s => s.EndTime == null)
                    })
                    .ToListAsync();

                return Ok(operatorStats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error retrieving operator stats", details = ex.Message });
            }
        }

        // Nuevo endpoint para vehículos más frecuentes
        [HttpGet("frequent-vehicles")]
        public async Task<ActionResult> GetFrequentVehicles()
        {
            try
            {
                var frequentVehicles = await _context.Vehicles
                    .Include(v => v.User)
                    .Include(v => v.Tickets)
                    .Where(v => v.Tickets.Count > 0)
                    .OrderByDescending(v => v.Tickets.Count)
                    .Take(10)
                    .Select(v => new
                    {
                        VehicleId = v.Id,
                        LicensePlate = v.Plate,
                        VehicleType = v.Type,
                        OwnerName = v.User.FullName,
                        TotalVisits = v.Tickets.Count,
                        LastVisit = v.Tickets.Max(t => t.EntryTime)
                    })
                    .ToListAsync();

                return Ok(frequentVehicles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error retrieving frequent vehicles", details = ex.Message });
            }
        }
    }
}