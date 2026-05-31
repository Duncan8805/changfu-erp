using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChangFuPOS.Data;
using ChangFuPOS.DTOs;
using ChangFuPOS.Models;

namespace ChangFuPOS.Controllers;

[ApiController]
[Route("api/rice-types")]
[Authorize]
public class RiceTypesController : ControllerBase
{
    private readonly AppDbContext _db;

    public RiceTypesController(AppDbContext db) => _db = db;

    /// <summary>GET /api/rice-types — 取得米種列表（含今日牌價）</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var riceTypes = await _db.RiceTypes
            .OrderBy(r => r.Id)
            .ToListAsync();

        var todayPrices = await _db.PriceLogs
            .Where(p => p.PriceDate == today)
            .ToDictionaryAsync(p => p.RiceTypeId, p => p.UnitPrice);

        var result = riceTypes.Select(r => new RiceTypeDto
        {
            Id = r.Id,
            Name = r.Name,
            IsActive = r.IsActive,
            TodayPrice = todayPrices.TryGetValue(r.Id, out var price) ? price : null
        });

        return Ok(result);
    }

    /// <summary>POST /api/rice-types — 新增米種</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RiceTypeRequest request)
    {
        var riceType = new RiceType
        {
            Name = request.Name,
            IsActive = request.IsActive
        };

        _db.RiceTypes.Add(riceType);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = riceType.Id }, new RiceTypeDto
        {
            Id = riceType.Id,
            Name = riceType.Name,
            IsActive = riceType.IsActive,
            TodayPrice = null
        });
    }

    /// <summary>PUT /api/rice-types/{id} — 修改米種</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] RiceTypeRequest request)
    {
        var riceType = await _db.RiceTypes.FindAsync(id);
        if (riceType == null) return NotFound();

        riceType.Name = request.Name;
        riceType.IsActive = request.IsActive;

        await _db.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var todayPrice = await _db.PriceLogs
            .Where(p => p.RiceTypeId == id && p.PriceDate == today)
            .Select(p => (decimal?)p.UnitPrice)
            .FirstOrDefaultAsync();

        return Ok(new RiceTypeDto
        {
            Id = riceType.Id,
            Name = riceType.Name,
            IsActive = riceType.IsActive,
            TodayPrice = todayPrice
        });
    }

    /// <summary>DELETE /api/rice-types/{id} — 刪除米種（有關聯 Ticket 則 409）</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var riceType = await _db.RiceTypes.FindAsync(id);
        if (riceType == null) return NotFound();

        var hasTickets = await _db.Tickets.AnyAsync(t => t.RiceTypeId == id);
        if (hasTickets)
        {
            return Conflict(new { message = "此米種已有關聯傳票，無法刪除，請改為停用" });
        }

        _db.RiceTypes.Remove(riceType);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
