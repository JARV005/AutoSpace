using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSpace.Data;
using AutoSpace.Models;
using AutoSpace.DTOs;

namespace AutoSpace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetSubscriptions()
        {
            var subscriptions = await _context.Subscriptions
                .Include(s => s.User)
                .Include(s => s.Vehicle)
                .Select(s => new SubscriptionDto
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    UserFullName = s.User.FullName,
                    VehicleId = s.VehicleId,
                    VehiclePlate = s.Vehicle.Plate,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    MonthlyPrice = s.MonthlyPrice,
                    Status = s.Status,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return subscriptions;
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetActiveSubscriptions()
        {
            var subscriptions = await _context.Subscriptions
                .Where(s => s.Status == "Active" && s.EndDate > DateTime.UtcNow)
                .Include(s => s.User)
                .Include(s => s.Vehicle)
                .Select(s => new SubscriptionDto
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    UserFullName = s.User.FullName,
                    VehicleId = s.VehicleId,
                    VehiclePlate = s.Vehicle.Plate,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    MonthlyPrice = s.MonthlyPrice,
                    Status = s.Status,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return subscriptions;
        }

        [HttpGet("expiring")]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetExpiringSubscriptions()
        {
            var expiringDate = DateTime.UtcNow.AddDays(7);
            var subscriptions = await _context.Subscriptions
                .Where(s => s.Status == "Active" && s.EndDate <= expiringDate)
                .Include(s => s.User)
                .Include(s => s.Vehicle)
                .Select(s => new SubscriptionDto
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    UserFullName = s.User.FullName,
                    VehicleId = s.VehicleId,
                    VehiclePlate = s.Vehicle.Plate,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    MonthlyPrice = s.MonthlyPrice,
                    Status = s.Status,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return subscriptions;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubscriptionDto>> GetSubscription(int id)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.User)
                .Include(s => s.Vehicle)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subscription == null)
            {
                return NotFound();
            }

            var subscriptionDto = new SubscriptionDto
            {
                Id = subscription.Id,
                UserId = subscription.UserId,
                UserFullName = subscription.User.FullName,
                VehicleId = subscription.VehicleId,
                VehiclePlate = subscription.Vehicle.Plate,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                MonthlyPrice = subscription.MonthlyPrice,
                Status = subscription.Status,
                CreatedAt = subscription.CreatedAt
            };

            return subscriptionDto;
        }

        [HttpPost]
        public async Task<ActionResult<SubscriptionDto>> CreateSubscription(CreateSubscriptionDto createSubscriptionDto)
        {
            // Verificar que el usuario existe
            var userExists = await _context.Users.AnyAsync(u => u.Id == createSubscriptionDto.UserId);
            if (!userExists)
            {
                return BadRequest(new { error = "El usuario especificado no existe" });
            }

            // Verificar que el vehículo existe
            var vehicleExists = await _context.Vehicles.AnyAsync(v => v.Id == createSubscriptionDto.VehicleId);
            if (!vehicleExists)
            {
                return BadRequest(new { error = "El vehículo especificado no existe" });
            }

            var subscription = new Subscription
            {
                UserId = createSubscriptionDto.UserId,
                VehicleId = createSubscriptionDto.VehicleId,
                StartDate = createSubscriptionDto.StartDate,
                EndDate = createSubscriptionDto.EndDate,
                MonthlyPrice = createSubscriptionDto.MonthlyPrice,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            // Cargar datos relacionados
            await _context.Entry(subscription).Reference(s => s.User).LoadAsync();
            await _context.Entry(subscription).Reference(s => s.Vehicle).LoadAsync();

            var subscriptionDto = new SubscriptionDto
            {
                Id = subscription.Id,
                UserId = subscription.UserId,
                UserFullName = subscription.User.FullName,
                VehicleId = subscription.VehicleId,
                VehiclePlate = subscription.Vehicle.Plate,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                MonthlyPrice = subscription.MonthlyPrice,
                Status = subscription.Status,
                CreatedAt = subscription.CreatedAt
            };

            return CreatedAtAction(nameof(GetSubscription), new { id = subscription.Id }, subscriptionDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubscription(int id, UpdateSubscriptionDto updateSubscriptionDto)
        {
            var subscription = await _context.Subscriptions.FindAsync(id);
            if (subscription == null)
            {
                return NotFound();
            }

            if (updateSubscriptionDto.EndDate.HasValue)
                subscription.EndDate = updateSubscriptionDto.EndDate.Value;

            if (updateSubscriptionDto.MonthlyPrice.HasValue)
                subscription.MonthlyPrice = updateSubscriptionDto.MonthlyPrice.Value;

            if (!string.IsNullOrEmpty(updateSubscriptionDto.Status))
                subscription.Status = updateSubscriptionDto.Status;

            await _context.SaveChangesAsync();

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

            subscription.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}