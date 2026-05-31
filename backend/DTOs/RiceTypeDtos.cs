namespace ChangFuPOS.DTOs;

// ─── Response DTO ───────────────────────────────────────────────
public class RiceTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsActive { get; set; }
    public decimal? TodayPrice { get; set; }
}

// ─── Create/Update Request ───────────────────────────────────────
public class RiceTypeRequest
{
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
