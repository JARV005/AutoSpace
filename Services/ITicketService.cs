using AutoSpace.Models;
using AutoSpace.DTOs;

namespace AutoSpace.Services
{
    public interface ITicketService
    {
        Task<Ticket> CreateEntryAsync(TicketDto ticketDto);
        Task<Ticket> ProcessExitAsync(TicketExitDto exitDto);
        decimal CalculateAmount(DateTime entryTime, DateTime exitTime, Rate rate);
    }
}