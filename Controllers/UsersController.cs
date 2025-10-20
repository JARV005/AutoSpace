using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSpace.Data;
using AutoSpace.Models;
using AutoSpace.DTOs;

namespace AutoSpace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Vehicles)
                .Include(u => u.Subscriptions)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Document = u.Document,
                    Email = u.Email,
                    Status = u.Status,
                    CreatedAt = u.CreatedAt,
                    VehicleCount = u.Vehicles.Count,
                    SubscriptionCount = u.Subscriptions.Count
                })
                .ToListAsync();

            return users;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Vehicles)
                .Include(u => u.Subscriptions)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Document = user.Document,
                Email = user.Email,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                VehicleCount = user.Vehicles.Count,
                SubscriptionCount = user.Subscriptions.Count
            };

            return userDto;
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
        {
            // Verificar si el documento o email ya existen
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Document == createUserDto.Document || u.Email == createUserDto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { error = "Ya existe un usuario con el mismo documento o email" });
            }

            var user = new User
            {
                FullName = createUserDto.FullName,
                Document = createUserDto.Document,
                Email = createUserDto.Email,
                Status = createUserDto.Status,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userDto = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Document = user.Document,
                Email = user.Email,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                VehicleCount = 0,
                SubscriptionCount = 0
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(updateUserDto.FullName))
                user.FullName = updateUserDto.FullName;

            if (!string.IsNullOrEmpty(updateUserDto.Document))
                user.Document = updateUserDto.Document;

            if (!string.IsNullOrEmpty(updateUserDto.Email))
                user.Email = updateUserDto.Email;

            if (!string.IsNullOrEmpty(updateUserDto.Status))
                user.Status = updateUserDto.Status;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}