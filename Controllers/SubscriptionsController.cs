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
    public class SubscriptionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<SubscriptionsController> _logger;

        public SubscriptionsController(ApplicationDbContext context, IEmailService emailService, ILogger<SubscriptionsController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubscriptionResponseDto>>> GetSubscriptions()
        {
            var subscriptions = await _context.Subscriptions
                .Include(s => s.User)
                .Include(s => s.Vehicle)
                .OrderByDescending(s => s.StartDate)
                .Select(s => new SubscriptionResponseDto
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    UserFullName = s.User.FullName,
                    VehicleId = s.VehicleId,
                    VehiclePlate = s.Vehicle.Plate,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    Status = s.Status,
                    MonthlyPrice = s.MonthlyPrice,
                    
                })
                .ToListAsync();

            return Ok(subscriptions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubscriptionResponseDto>> GetSubscription(int id)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.User)
                .Include(s => s.Vehicle)
                .Where(s => s.Id == id)
                .Select(s => new SubscriptionResponseDto
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    UserFullName = s.User.FullName,
                    VehicleId = s.VehicleId,
                    VehiclePlate = s.Vehicle.Plate,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    Status = s.Status,
                    MonthlyPrice = s.MonthlyPrice,
                  
                })
                .FirstOrDefaultAsync();

            if (subscription == null)
            {
                return NotFound();
            }

            return subscription;
        }

        [HttpPost]
        public async Task<ActionResult<Subscription>> CreateSubscription(SubscriptionDto subscriptionDto)
        {
            // Validate if there's an active subscription for the same vehicle
            var existingActive = await _context.Subscriptions
                .AnyAsync(s => s.VehicleId == subscriptionDto.VehicleId && 
                              s.Status == "Active" && 
                              s.EndDate >= DateTime.UtcNow);

            if (existingActive)
            {
                return BadRequest("Ya existe una mensualidad activa para este vehículo");
            }

            var subscription = new Subscription
            {
                UserId = subscriptionDto.UserId,
                VehicleId = subscriptionDto.VehicleId,
                StartDate = subscriptionDto.StartDate.Date,
                EndDate = subscriptionDto.EndDate.Date,
                MonthlyPrice = subscriptionDto.MonthlyPrice,
                Status = "Active"
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            // Send confirmation email
            try
            {
                await _emailService.SendSubscriptionConfirmation(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending subscription confirmation email");
            }

            return CreatedAtAction(nameof(GetSubscription), new { id = subscription.Id }, subscription);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubscription(int id, SubscriptionDto subscriptionDto)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null)
            {
                return NotFound();
            }

            // Validate if there's another active subscription for the same vehicle
            var existingActive = await _context.Subscriptions
                .AnyAsync(s => s.VehicleId == subscriptionDto.VehicleId && 
                              s.Id != id && 
                              s.Status == "Active" && 
                              s.EndDate >= DateTime.UtcNow);

            if (existingActive)
            {
                return BadRequest("Ya existe otra mensualidad activa para este vehículo");
            }

            subscription.UserId = subscriptionDto.UserId;
            subscription.VehicleId = subscriptionDto.VehicleId;
            subscription.StartDate = subscriptionDto.StartDate.Date;
            subscription.EndDate = subscriptionDto.EndDate.Date;
            subscription.MonthlyPrice = subscriptionDto.MonthlyPrice;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SubscriptionExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubscription(int id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null)
            {
                return NotFound();
            }

            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> DeactivateSubscription(int id)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null)
            {
                return NotFound();
            }

            subscription.Status = "Inactive";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("expiring")]
        public async Task<ActionResult<IEnumerable<SubscriptionResponseDto>>> GetExpiringSubscriptions()
        {
            var threeDaysFromNow = DateTime.UtcNow.AddDays(3);
            var subscriptions = await _context.Subscriptions
                .Include(s => s.User)
                .Include(s => s.Vehicle)
                .Where(s => s.Status == "Active" && 
                           s.EndDate <= threeDaysFromNow && 
                           s.EndDate >= DateTime.UtcNow.Date)
                .Select(s => new SubscriptionResponseDto
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    UserFullName = s.User.FullName,
                    VehicleId = s.VehicleId,
                    VehiclePlate = s.Vehicle.Plate,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    Status = s.Status,
                    MonthlyPrice = s.MonthlyPrice,
                    
                })
                .OrderBy(s => s.EndDate)
                .ToListAsync();

            return Ok(subscriptions);
        }

        [HttpPost("send-expiration-warnings")]
        public async Task<IActionResult> SendExpirationWarnings()
        {
            var expiringSubscriptions = await _context.Subscriptions
                .Include(s => s.User)
                .Where(s => s.Status == "Active" && 
                           s.EndDate <= DateTime.UtcNow.AddDays(3) && 
                           s.EndDate >= DateTime.UtcNow.Date &&
                           s.User.Email != null)
                .ToListAsync();

            var sentCount = 0;
            foreach (var subscription in expiringSubscriptions)
            {
                try
                {
                    await _emailService.SendSubscriptionExpirationWarning(subscription);
                    sentCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending expiration warning for subscription {SubscriptionId}", subscription.Id);
                }
            }

            return Ok(new { SentCount = sentCount, Total = expiringSubscriptions.Count });
        }

        private bool SubscriptionExists(int id)
        {
            return _context.Subscriptions.Any(e => e.Id == id);
        }

        private static string GetSubscriptionStatus(Subscription subscription)
        {
            var now = DateTime.UtcNow.Date;
            
            if (subscription.Status != "Active") return "Inactive";
            if (subscription.EndDate < now) return "Expired";
            if (subscription.StartDate > now) return "Pending";
            if (subscription.EndDate <= now.AddDays(3)) return "AboutToExpire";
            
            return "Active";
        }
    }
}