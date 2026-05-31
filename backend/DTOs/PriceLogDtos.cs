namespace ChangFuPOS.DTOs;

// ─── Response DTO ───────────────────────────────────────────────
public class PriceLogDto
{
    public int Id { get; set; }
    public int RiceTypeId { get; set; }
    public string RiceTypeName { get; set; } = "";
    public string PriceDate { get; set; } = "";   // ISO date string "yyyy-MM-dd"
    public decimal UnitPrice { get; set; }
    public string CreatedBy { get; set; } = "";
}

// ─── Upsert Request ──────────────────────────────────────────────
public class UpsertPriceLogRequest
{
    public int RiceTypeId { get; set; }
    public string PriceDate { get; set; } = "";   // "yyyy-MM-dd"
    public decimal UnitPrice { get; set; }
}

// ─── Dashboard Summary ───────────────────────────────────────────
public class DashboardSummaryDto
{
    public int TotalVehicles { get; set; }
    public decimal TotalNetWeightKg { get; set; }
    public decimal TotalAmount { get; set; }
    public int ExceptionCount { get; set; }
}
