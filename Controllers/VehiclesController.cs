using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSpace.Data;
using AutoSpace.Models;
using AutoSpace.DTOs;

namespace AutoSpace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VehiclesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleDto>>> GetVehicles()
        {
            var vehicles = await _context.Vehicles
                .Include(v => v.User)
                .Include(v => v.Subscriptions)
                .Include(v => v.Tickets)
                .Select(v => new VehicleDto
                {
                    Id = v.Id,
                    Plate = v.Plate,
                    Type = v.Type,
                    UserId = v.UserId,
                    UserFullName = v.User.FullName,
                    CreatedAt = v.CreatedAt,
                    TicketCount = v.Tickets.Count,
                    HasActiveSubscription = v.Subscriptions.Any(s => s.Status == "Active" && s.EndDate > DateTime.UtcNow)
                })
                .ToListAsync();

            return vehicles;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleDto>> GetVehicle(int id)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.User)
                .Include(v => v.Subscriptions)
                .Include(v => v.Tickets)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
            {
                return NotFound();
            }

            var vehicleDto = new VehicleDto
            {
                Id = vehicle.Id,
                Plate = vehicle.Plate,
                Type = vehicle.Type,
                UserId = vehicle.UserId,
                UserFullName = vehicle.User.FullName,
                CreatedAt = vehicle.CreatedAt,
                TicketCount = vehicle.Tickets.Count,
                HasActiveSubscription = vehicle.Subscriptions.Any(s => s.Status == "Active" && s.EndDate > DateTime.UtcNow)
            };

            return vehicleDto;
        }

        [HttpPost]
        public async Task<ActionResult<VehicleDto>> CreateVehicle(CreateVehicleDto createVehicleDto)
        {
            // Verificar si el usuario existe
            var userExists = await _context.Users.AnyAsync(u => u.Id == createVehicleDto.UserId);
            if (!userExists)
            {
                return BadRequest(new { error = "El usuario especificado no existe" });
            }

            // Verificar si la placa ya existe
            var existingVehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Plate == createVehicleDto.Plate);
                
            if (existingVehicle != null)
            {
                return BadRequest(new { error = "Ya existe un vehículo con esta placa" });
            }

            var vehicle = new Vehicle
            {
                Plate = createVehicleDto.Plate,
                Type = createVehicleDto.Type,
                UserId = createVehicleDto.UserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            // Cargar datos relacionados
            await _context.Entry(vehicle).Reference(v => v.User).LoadAsync();

            var vehicleDto = new VehicleDto
            {
                Id = vehicle.Id,
                Plate = vehicle.Plate,
                Type = vehicle.Type,
                UserId = vehicle.UserId,
                UserFullName = vehicle.User.FullName,
                CreatedAt = vehicle.CreatedAt,
                TicketCount = 0,
                HasActiveSubscription = false
            };

            return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, vehicleDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, UpdateVehicleDto updateVehicleDto)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(updateVehicleDto.Plate))
                vehicle.Plate = updateVehicleDto.Plate;

            if (!string.IsNullOrEmpty(updateVehicleDto.Type))
                vehicle.Type = updateVehicleDto.Type;

            if (updateVehicleDto.UserId.HasValue)
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == updateVehicleDto.UserId.Value);
                if (!userExists)
                {
                    return BadRequest(new { error = "El usuario especificado no existe" });
                }
                vehicle.UserId = updateVehicleDto.UserId.Value;
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}