using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChangFuPOS.Data;
using ChangFuPOS.DTOs;

namespace ChangFuPOS.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db) => _db = db;

    /// <summary>GET /api/dashboard/summary — 取得報表摘要</summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] string? dateFrom,
        [FromQuery] string? dateTo)
    {
        var query = _db.Tickets
            .Where(t => t.Status == "settled")
            .AsQueryable();

        if (!string.IsNullOrEmpty(dateFrom) && DateTime.TryParse(dateFrom, out var from))
        {
            var fromUtc = from.ToUniversalTime();
            query = query.Where(t => t.SettledAt >= fromUtc);
        }

        if (!string.IsNullOrEmpty(dateTo) && DateTime.TryParse(dateTo, out var to))
        {
            var toUtc = to.ToUniversalTime().AddDays(1);
            query = query.Where(t => t.SettledAt < toUtc);
        }

        var tickets = await query.ToListAsync();

        var summary = new DashboardSummaryDto
        {
            TotalVehicles = tickets.Count,
            TotalNetWeightKg = tickets.Sum(t => t.NetWeightKg),
            TotalAmount = tickets.Sum(t => t.TotalAmount),
            ExceptionCount = tickets.Count(t => t.IsException)
        };

        return Ok(summary);
    }
}
