// Controllers/ReportsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoSpace.Data;
using AutoSpace.Models;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/reports/income?startDate=2024-01-01&endDate=2024-01-31
    [HttpGet("income")]
    public async Task<ActionResult<IncomeReportDto>> GetIncomeReport(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        try
        {
            var subscriptions = await _context.Subscriptions
                .Where(s => s.StartDate >= startDate && s.EndDate <= endDate)
                .Include(s => s.Vehicle)
                .Include(s => s.User)
                .ToListAsync();

            var totalIncome = subscriptions.Sum(s => s.MonthlyPrice);
            var totalVehicles = subscriptions.Count;

            var dailyIncomes = subscriptions
                .GroupBy(s => s.StartDate.Date)
                .Select(g => new DailyIncomeDto
                {
                    Date = g.Key,
                    Amount = g.Sum(s => s.MonthlyPrice),
                    VehicleCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            var report = new IncomeReportDto
            {
                TotalIncome = totalIncome,
                TotalVehicles = totalVehicles,
                DailyIncomes = dailyIncomes,
                StartDate = startDate,
                EndDate = endDate
            };

            return Ok(report);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error generando reporte de ingresos: {ex.Message}");
        }
    }

    // GET: api/reports/subscriptions?startDate=2024-01-01&endDate=2024-01-31
    [HttpGet("subscriptions")]
    public async Task<ActionResult<SubscriptionReportDto>> GetSubscriptionsReport(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        try
        {
            var subscriptions = await _context.Subscriptions
                .Where(s => s.StartDate >= startDate && s.EndDate <= endDate)
                .Include(s => s.Vehicle)
                .Include(s => s.User)
                .Select(s => new SubscriptionDetailDto
                {
                    Id = s.Id,
                    VehiclePlate = s.Vehicle.Plate,
                    VehicleType = s.Vehicle.Type,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    MonthlyPrice = s.MonthlyPrice,
                    Status = s.Status,
                    UserName = s.User.FullName // Usar FullName
                })
                .ToListAsync();

            var report = new SubscriptionReportDto
            {
                TotalSubscriptions = subscriptions.Count,
                ActiveSubscriptions = subscriptions.Count(s => s.Status == "Active"),
                ExpiredSubscriptions = subscriptions.Count(s => s.Status == "Expired" || s.Status == "Cancelled"),
                TotalRevenue = subscriptions.Sum(s => s.MonthlyPrice),
                Subscriptions = subscriptions,
                StartDate = startDate,
                EndDate = endDate
            };

            return Ok(report);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error generando reporte de suscripciones: {ex.Message}");
        }
    }

    // GET: api/reports/users?startDate=2024-01-01&endDate=2024-01-31
    [HttpGet("users")]
    public async Task<ActionResult<UserReportDto>> GetUsersReport(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        try
        {
            var users = await _context.Users
                .Include(u => u.Subscriptions)
                .Select(u => new UserPerformanceDto
                {
                    UserId = u.Id,
                    UserName = u.FullName, // Usar FullName
                    Email = u.Email,
                    Document = u.Document,
                    Status = u.Status,
                    TotalSubscriptions = u.Subscriptions.Count(s => 
                        s.StartDate >= startDate && s.EndDate <= endDate),
                    TotalRevenue = u.Subscriptions
                        .Where(s => s.StartDate >= startDate && s.EndDate <= endDate)
                        .Sum(s => s.MonthlyPrice)
                })
                .ToListAsync();

            var report = new UserReportDto
            {
                TotalUsers = users.Count,
                ActiveUsers = users.Count(u => u.Status == "Active"),
                UserPerformances = users.Where(u => u.TotalSubscriptions > 0).ToList(),
                StartDate = startDate,
                EndDate = endDate
            };

            return Ok(report);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error generando reporte de usuarios: {ex.Message}");
        }
    }

    // GET: api/reports/operators?startDate=2024-01-01&endDate=2024-01-31
    [HttpGet("operators")]
    public async Task<ActionResult<OperatorReportDto>> GetOperatorsReport(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        try
        {
            var operators = await _context.Operators
                .Select(o => new OperatorPerformanceDto
                {
                    OperatorId = o.Id,
                    OperatorName = o.FullName, // Usar FullName
                    Email = o.Email,
                    Document = o.Document,
                    Status = o.Status,
                    IsActive = o.IsActive,
                    TotalTickets = o.Tickets.Count,
                    TotalPayments = o.Payments.Count,
                    TotalShifts = o.Shifts.Count
                })
                .ToListAsync();

            var report = new OperatorReportDto
            {
                TotalOperators = operators.Count,
                ActiveOperators = operators.Count(o => o.IsActive),
                OperatorPerformances = operators,
                StartDate = startDate,
                EndDate = endDate
            };

            return Ok(report);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error generando reporte de operadores: {ex.Message}");
        }
    }

    // GET: api/reports/vehicles?startDate=2024-01-01&endDate=2024-01-31
    [HttpGet("vehicles")]
    public async Task<ActionResult<VehicleReportDto>> GetVehiclesReport(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        try
        {
            var vehicles = await _context.Vehicles
                .Include(v => v.Subscriptions)
                .Where(v => v.Subscriptions.Any(s => 
                    s.StartDate >= startDate && s.EndDate <= endDate))
                .Select(v => new VehicleStatsDto
                {
                    VehicleId = v.Id,
                    Plate = v.Plate,
                    Type = v.Type,
                    TotalSubscriptions = v.Subscriptions.Count(s => 
                        s.StartDate >= startDate && s.EndDate <= endDate),
                    TotalRevenue = v.Subscriptions
                        .Where(s => s.StartDate >= startDate && s.EndDate <= endDate)
                        .Sum(s => s.MonthlyPrice)
                })
                .ToListAsync();

            var report = new VehicleReportDto
            {
                TotalVehicles = vehicles.Count,
                VehicleStats = vehicles,
                StartDate = startDate,
                EndDate = endDate
            };

            return Ok(report);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error generando reporte de vehículos: {ex.Message}");
        }
    }
}