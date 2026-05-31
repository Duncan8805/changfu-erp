using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChangFuPOS.Data;
using ChangFuPOS.DTOs;
using ChangFuPOS.Models;

namespace ChangFuPOS.Controllers;

[ApiController]
[Route("api/price-logs")]
[Authorize]
public class PriceLogsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PriceLogsController(AppDbContext db) => _db = db;

    /// <summary>GET /api/price-logs — 查詢牌價歷史</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? riceTypeId,
        [FromQuery] string? dateFrom,
        [FromQuery] string? dateTo)
    {
        var query = _db.PriceLogs
            .Include(p => p.RiceType)
            .AsQueryable();

        if (riceTypeId.HasValue)
            query = query.Where(p => p.RiceTypeId == riceTypeId.Value);

        if (!string.IsNullOrEmpty(dateFrom) && DateOnly.TryParse(dateFrom, out var from))
            query = query.Where(p => p.PriceDate >= from);

        if (!string.IsNullOrEmpty(dateTo) && DateOnly.TryParse(dateTo, out var to))
            query = query.Where(p => p.PriceDate <= to);

        var logs = await query
            .OrderByDescending(p => p.PriceDate)
            .ThenBy(p => p.RiceTypeId)
            .ToListAsync();

        var result = logs.Select(p => new PriceLogDto
        {
            Id = p.Id,
            RiceTypeId = p.RiceTypeId,
            RiceTypeName = p.RiceType.Name,
            PriceDate = p.PriceDate.ToString("yyyy-MM-dd"),
            UnitPrice = p.UnitPrice,
            CreatedBy = p.CreatedBy
        });

        return Ok(result);
    }

    /// <summary>POST /api/price-logs — Upsert 牌價（同天同米種則更新）</summary>
    [HttpPost]
    public async Task<IActionResult> Upsert([FromBody] UpsertPriceLogRequest request)
    {
        if (!DateOnly.TryParse(request.PriceDate, out var priceDate))
            return BadRequest(new { message = "日期格式錯誤，請使用 yyyy-MM-dd" });

        var username = User.Identity?.Name ?? "system";

        var existing = await _db.PriceLogs
            .FirstOrDefaultAsync(p => p.RiceTypeId == request.RiceTypeId && p.PriceDate == priceDate);

        bool isNew = existing == null;

        if (isNew)
        {
            existing = new PriceLog
            {
                RiceTypeId = request.RiceTypeId,
                PriceDate = priceDate,
                UnitPrice = request.UnitPrice,
                CreatedBy = username
            };
            _db.PriceLogs.Add(existing);
        }
        else
        {
            existing!.UnitPrice = request.UnitPrice;
        }

        await _db.SaveChangesAsync();

        // Reload with navigation
        await _db.Entry(existing).Reference(p => p.RiceType).LoadAsync();

        var dto = new PriceLogDto
        {
            Id = existing.Id,
            RiceTypeId = existing.RiceTypeId,
            RiceTypeName = existing.RiceType.Name,
            PriceDate = existing.PriceDate.ToString("yyyy-MM-dd"),
            UnitPrice = existing.UnitPrice,
            CreatedBy = existing.CreatedBy
        };

        return isNew ? Created($"/api/price-logs/{existing.Id}", dto) : Ok(dto);
    }
}
