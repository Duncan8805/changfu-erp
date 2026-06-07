using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChangFuPOS.Data;
using ChangFuPOS.Models;

namespace ChangFuPOS.Controllers;

[ApiController]
[Route("api/note-presets")]
public class NotePresetsController : ControllerBase
{
    private readonly AppDbContext _db;
    public NotePresetsController(AppDbContext db) => _db = db;

    // ─── GET /api/note-presets ────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var presets = await _db.NotePresets
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.CreatedAt)
            .ToListAsync();
        return Ok(presets);
    }

    // ─── POST /api/note-presets ───────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNotePresetRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(new { message = "內容不可為空" });

        var preset = new NotePreset
        {
            Content   = request.Content.Trim(),
            SortOrder = request.SortOrder,
        };
        _db.NotePresets.Add(preset);
        await _db.SaveChangesAsync();
        return Ok(preset);
    }

    // ─── DELETE /api/note-presets/{id} ───────────────────────────
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var preset = await _db.NotePresets.FindAsync(id);
        if (preset == null) return NotFound();
        _db.NotePresets.Remove(preset);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

// ─── Request DTO ────────────────────────────────────────────────
public class CreateNotePresetRequest
{
    public string Content   { get; set; } = "";
    public int    SortOrder { get; set; } = 0;
}
