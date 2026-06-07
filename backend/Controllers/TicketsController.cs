using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChangFuPOS.Data;
using ChangFuPOS.DTOs;
using ChangFuPOS.Models;

namespace ChangFuPOS.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TicketsController(AppDbContext db) => _db = db;

    // ─── Helper: Map Ticket → TicketDto ─────────────────────────
    private static TicketDto MapToDto(Ticket t) => new()
    {
        Id = t.Id,
        TicketNo = t.TicketNo,
        VehicleNo = t.VehicleNo,
        FarmerName = t.FarmerName,
        Village = t.Village,
        RiceTypeId = t.RiceTypeId,
        RiceTypeName = t.RiceType?.Name ?? "",
        GrossWeightKg = t.GrossWeightKg,
        TareWeightKg = t.TareWeightKg,
        NetWeightKg = t.NetWeightKg,
        NetWeightJin = t.NetWeightJin,
        PriceSnapshot = t.PriceSnapshot,
        TotalAmount = t.TotalAmount,
        IsException = t.IsException,
        ExceptionReason = t.ExceptionReason,
        Note = t.Note,
        Status = t.Status,
        SettledAt = t.SettledAt,
        CreatedBy = t.CreatedBy,
        CreatedAt = t.CreatedAt
    };

    // ─── GET /api/tickets ────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] string? dateFrom,
        [FromQuery] string? dateTo,
        [FromQuery] bool? isException)
    {
        var query = _db.Tickets
            .Include(t => t.RiceType)
            .Where(t => !t.IsDeleted)   // 排除軟刪除
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(t => t.Status == status);

        if (isException.HasValue)
            query = query.Where(t => t.IsException == isException.Value);

        if (!string.IsNullOrEmpty(dateFrom) && DateTime.TryParse(dateFrom, out var from))
        {
            var fromUtc = from.ToUniversalTime();
            query = query.Where(t =>
                (t.SettledAt.HasValue ? t.SettledAt >= fromUtc : t.CreatedAt >= fromUtc));
        }

        if (!string.IsNullOrEmpty(dateTo) && DateTime.TryParse(dateTo, out var to))
        {
            var toUtc = to.ToUniversalTime().AddDays(1); // inclusive end
            query = query.Where(t =>
                (t.SettledAt.HasValue ? t.SettledAt < toUtc : t.CreatedAt < toUtc));
        }

        var tickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(tickets.Select(MapToDto));
    }

    // ─── GET /api/tickets/{id} ───────────────────────────────────
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ticket = await _db.Tickets
            .Include(t => t.RiceType)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null) return NotFound();

        return Ok(MapToDto(ticket));
    }

    // ─── POST /api/tickets ───────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTicketRequest request)
    {
        var username = User.Identity?.Name ?? "system";

        // 產生流水號（查今日最大 id + 1）
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);
        var todayCount = await _db.Tickets
            .CountAsync(t => t.CreatedAt >= todayStart && t.CreatedAt < todayEnd);

        var ticketNo = $"No.{(todayCount + 1):D6}";

        // 建立時預設用米種 1（未選擇），settle 時才確認
        var defaultRiceType = await _db.RiceTypes.FirstAsync();

        var ticket = new Ticket
        {
            TicketNo = ticketNo,
            VehicleNo = request.VehicleNo,
            FarmerName = request.FarmerName,
            Village = request.Village,
            GrossWeightKg = request.GrossWeightKg,
            Status = "unloading",
            CreatedBy = username,
            CreatedAt = DateTime.UtcNow,
            RiceTypeId = defaultRiceType.Id
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();

        await _db.Entry(ticket).Reference(t => t.RiceType).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, MapToDto(ticket));
    }

    // ─── PUT /api/tickets/{id} ───────────────────────────────────
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTicketRequest request)
    {
        var ticket = await _db.Tickets
            .Include(t => t.RiceType)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null) return NotFound();

        if (request.VehicleNo != null) ticket.VehicleNo = request.VehicleNo;
        if (request.FarmerName != null) ticket.FarmerName = request.FarmerName;
        if (request.Village != null) ticket.Village = request.Village;
        if (request.RiceTypeId.HasValue) ticket.RiceTypeId = request.RiceTypeId.Value;
        if (request.GrossWeightKg.HasValue) ticket.GrossWeightKg = request.GrossWeightKg.Value;
        if (request.TareWeightKg.HasValue) ticket.TareWeightKg = request.TareWeightKg.Value;
        if (request.IsException.HasValue) ticket.IsException = request.IsException.Value;
        if (request.ExceptionReason != null) ticket.ExceptionReason = request.ExceptionReason;
        if (request.Note != null) ticket.Note = request.Note;

        // 後端自動計算淨重（不靠前端）
        if (ticket.GrossWeightKg > 0 && ticket.TareWeightKg > 0)
        {
            ticket.NetWeightKg = ticket.GrossWeightKg - ticket.TareWeightKg;
            ticket.NetWeightJin = Math.Round(ticket.NetWeightKg / 0.6m, 0);
        }

        await _db.SaveChangesAsync();

        // Reload navigation if RiceTypeId changed
        await _db.Entry(ticket).Reference(t => t.RiceType).LoadAsync();

        return Ok(MapToDto(ticket));
    }

    // ─── PATCH /api/tickets/{id}/status ─────────────────────────
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var ticket = await _db.Tickets
            .Include(t => t.RiceType)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null) return NotFound();

        // 只能往前推進狀態（unloading → pending → settled）
        var allowedTransitions = new Dictionary<string, IEnumerable<string>>
        {
            ["unloading"] = new[] { "pending" },
            ["pending"] = new[] { "unloading", "settled" },
            ["settled"] = Array.Empty<string>()
        };

        if (!allowedTransitions.TryGetValue(ticket.Status, out var allowed) ||
            !allowed.Contains(request.Status))
        {
            return BadRequest(new { message = $"無法從 {ticket.Status} 轉換為 {request.Status}" });
        }

        ticket.Status = request.Status;
        await _db.SaveChangesAsync();

        return Ok(MapToDto(ticket));
    }

    // ─── POST /api/tickets/{id}/settle ──────────────────────────
    [HttpPost("{id:int}/settle")]
    public async Task<IActionResult> Settle(int id, [FromBody] SettleTicketRequest request)
    {
        var ticket = await _db.Tickets
            .Include(t => t.RiceType)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        if (ticket == null) return NotFound();

        if (ticket.Status == "settled")
            return BadRequest(new { message = "此傳票已結算" });

        if (request.PriceOverride <= 0)
            return BadRequest(new { message = "單價必須大於 0" });

        // 例外單必須有原因
        if (request.IsException && string.IsNullOrWhiteSpace(request.ExceptionReason))
            return BadRequest(new { message = "例外單價必須填寫例外原因" });

        // 直接使用使用者輸入的單價（不查牌價表）
        ticket.PriceSnapshot = request.PriceOverride;
        ticket.IsException = request.IsException;
        ticket.ExceptionReason = request.ExceptionReason;
        ticket.Note = request.Note;

        // 重算（確保後端數字為準）
        ticket.NetWeightKg  = ticket.GrossWeightKg - ticket.TareWeightKg;
        ticket.NetWeightJin = Math.Round(ticket.NetWeightKg / 0.6m, 0);
        ticket.TotalAmount  = Math.Round(ticket.NetWeightJin * ticket.PriceSnapshot, 2);

        ticket.Status    = "settled";
        ticket.SettledAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _db.Entry(ticket).Reference(t => t.RiceType).LoadAsync();

        return Ok(MapToDto(ticket));
    }

    // ─── GET /api/tickets/trash ──────────────────────────────────
    [HttpGet("trash")]
    public async Task<IActionResult> GetTrash()
    {
        var cutoff = DateTime.UtcNow.AddDays(-7);
        var trashed = await _db.Tickets
            .Include(t => t.RiceType)
            .Where(t => t.IsDeleted && t.DeletedAt >= cutoff)
            .OrderByDescending(t => t.DeletedAt)
            .ToListAsync();
        return Ok(trashed.Select(MapToDto));
    }

    // ─── POST /api/tickets/{id}/restore ─────────────────────────
    [HttpPost("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id)
    {
        var ticket = await _db.Tickets
            .Include(t => t.RiceType)
            .FirstOrDefaultAsync(t => t.Id == id && t.IsDeleted);

        if (ticket == null) return NotFound();

        var cutoff = DateTime.UtcNow.AddDays(-7);
        if (ticket.DeletedAt < cutoff)
            return Conflict(new { message = "已超過 7 天，無法恢復" });

        ticket.IsDeleted = false;
        ticket.DeletedAt = null;
        await _db.SaveChangesAsync();

        return Ok(MapToDto(ticket));
    }

    // ─── DELETE /api/tickets/{id}（軟刪除，7天可恢復）────────────
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket == null) return NotFound();

        if (ticket.Status == "settled")
            return Conflict(new { message = "已結算的傳票無法刪除" });

        ticket.IsDeleted = true;
        ticket.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent(); // 204
    }
}
