using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSpace.Data;
using AutoSpace.Models;

namespace AutoSpace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Mail>>> GetMails()
        {
            return await _context.Mails
                .Include(m => m.User)
                .Include(m => m.Subscription)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Mail>> CreateMail(Mail mail)
        {
            mail.SentAt = DateTime.UtcNow;
            _context.Mails.Add(mail);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMails), new { id = mail.Id }, mail);
        }
    }
}