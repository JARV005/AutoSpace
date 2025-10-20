using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSpace.Data;
using AutoSpace.Models;
using AutoSpace.DTOs;

namespace AutoSpace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RateDto>>> GetRates()
        {
            var rates = await _context.Rates
                .Select(r => new RateDto
                {
                    Id = r.Id,
                    TypeVehicle = r.TypeVehicle,
                    HourPrice = r.HourPrice,
                    AddPrice = r.AddPrice,
                    MaxPrice = r.MaxPrice,
                    GraceTime = r.GraceTime,
                    IsActive = r.IsActive,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return rates;
        }

        [HttpGet("current")]
        public async Task<ActionResult<IEnumerable<RateDto>>> GetCurrentRates()
        {
            var rates = await _context.Rates
                .Where(r => r.IsActive)
                .Select(r => new RateDto
                {
                    Id = r.Id,
                    TypeVehicle = r.TypeVehicle,
                    HourPrice = r.HourPrice,
                    AddPrice = r.AddPrice,
                    MaxPrice = r.MaxPrice,
                    GraceTime = r.GraceTime,
                    IsActive = r.IsActive,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return rates;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RateDto>> GetRate(int id)
        {
            var rate = await _context.Rates.FindAsync(id);
            if (rate == null)
            {
                return NotFound();
            }

            var rateDto = new RateDto
            {
                Id = rate.Id,
                TypeVehicle = rate.TypeVehicle,
                HourPrice = rate.HourPrice,
                AddPrice = rate.AddPrice,
                MaxPrice = rate.MaxPrice,
                GraceTime = rate.GraceTime,
                IsActive = rate.IsActive,
                CreatedAt = rate.CreatedAt
            };

            return rateDto;
        }

        [HttpPost]
        public async Task<ActionResult<RateDto>> CreateRate(CreateRateDto createRateDto)
        {
            var rate = new Rate
            {
                TypeVehicle = createRateDto.TypeVehicle,
                HourPrice = createRateDto.HourPrice,
                AddPrice = createRateDto.AddPrice,
                MaxPrice = createRateDto.MaxPrice,
                GraceTime = createRateDto.GraceTime,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Rates.Add(rate);
            await _context.SaveChangesAsync();

            var rateDto = new RateDto
            {
                Id = rate.Id,
                TypeVehicle = rate.TypeVehicle,
                HourPrice = rate.HourPrice,
                AddPrice = rate.AddPrice,
                MaxPrice = rate.MaxPrice,
                GraceTime = rate.GraceTime,
                IsActive = rate.IsActive,
                CreatedAt = rate.CreatedAt
            };

            return CreatedAtAction(nameof(GetRate), new { id = rate.Id }, rateDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRate(int id, UpdateRateDto updateRateDto)
        {
            var rate = await _context.Rates.FindAsync(id);
            if (rate == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(updateRateDto.TypeVehicle))
                rate.TypeVehicle = updateRateDto.TypeVehicle;

            if (updateRateDto.HourPrice.HasValue)
                rate.HourPrice = updateRateDto.HourPrice.Value;

            if (updateRateDto.AddPrice.HasValue)
                rate.AddPrice = updateRateDto.AddPrice.Value;

            if (updateRateDto.MaxPrice.HasValue)
                rate.MaxPrice = updateRateDto.MaxPrice.Value;

            if (updateRateDto.GraceTime.HasValue)
                rate.GraceTime = updateRateDto.GraceTime.Value;

            if (updateRateDto.IsActive.HasValue)
                rate.IsActive = updateRateDto.IsActive.Value;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRate(int id)
        {
            var rate = await _context.Rates.FindAsync(id);
            if (rate == null)
            {
                return NotFound();
            }

            _context.Rates.Remove(rate);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}