using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSpace.Data;
using AutoSpace.Models;

namespace AutoSpace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VehiclesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehicles()
        {
            return await _context.Vehicles
                .Include(v => v.User)
                .OrderBy(v => v.Plate)
                .ToListAsync();
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetActiveVehicles()
        {
            return await _context.Vehicles
                .Include(v => v.User)
                .Where(v => v.User.Status == "Active") // Solo vehículos de usuarios activos
                .OrderBy(v => v.Plate)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Vehicle>> GetVehicle(int id)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
            {
                return NotFound();
            }

            return vehicle;
        }

        [HttpGet("plate/{plate}")]
        public async Task<ActionResult<Vehicle>> GetVehicleByPlate(string plate)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Plate == plate.ToUpper());

            if (vehicle == null)
            {
                return NotFound();
            }

            return vehicle;
        }

        [HttpPost]
        public async Task<ActionResult<Vehicle>> CreateVehicle(Vehicle vehicle)
        {
            // Normalizar placa
            vehicle.Plate = vehicle.Plate.ToUpper();

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, vehicle);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, Vehicle vehicle)
        {
            if (id != vehicle.Id)
            {
                return BadRequest();
            }

            // Normalizar placa
            vehicle.Plate = vehicle.Plate.ToUpper();

            _context.Entry(vehicle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleExists(id))
                {
                    return NotFound();
                }
                throw;
            }

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

        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.Id == id);
        }
    }
}