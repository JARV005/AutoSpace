using AutoSpace.Models;
using AutoSpace.DTOs;

namespace AutoSpace.Services
{
    public interface ITicketService
    {
        Task<Ticket> RegisterEntryAsync(CreateTicketDto createTicketDto);
        Task<Ticket> RegisterExitAsync(ExitTicketDto exitTicketDto);
        Task<decimal> CalculateAmountAsync(DateTime entryTime, DateTime exitTime, string vehicleType);
    }
}