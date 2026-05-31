namespace ChangFuPOS.DTOs;

// ─── Response DTO ───────────────────────────────────────────────
public class TicketDto
{
    public int Id { get; set; }
    public string TicketNo { get; set; } = "";
    public string VehicleNo { get; set; } = "";
    public string FarmerName { get; set; } = "";
    public string Village { get; set; } = "";
    public int RiceTypeId { get; set; }
    public string RiceTypeName { get; set; } = "";
    public decimal GrossWeightKg { get; set; }
    public decimal TareWeightKg { get; set; }
    public decimal NetWeightKg { get; set; }
    public decimal NetWeightJin { get; set; }
    public decimal PriceSnapshot { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsException { get; set; }
    public string? ExceptionReason { get; set; }
    public string? Note { get; set; }
    public string Status { get; set; } = "";
    public DateTime? SettledAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

// ─── Create Request ──────────────────────────────────────────────
public class CreateTicketRequest
{
    public string VehicleNo { get; set; } = "";
    public string FarmerName { get; set; } = "";
    public string Village { get; set; } = "";
    public decimal GrossWeightKg { get; set; }
}

// ─── Update Request ──────────────────────────────────────────────
public class UpdateTicketRequest
{
    public string? VehicleNo { get; set; }
    public string? FarmerName { get; set; }
    public string? Village { get; set; }
    public int? RiceTypeId { get; set; }
    public decimal? GrossWeightKg { get; set; }
    public decimal? TareWeightKg { get; set; }
    public bool? IsException { get; set; }
    public string? ExceptionReason { get; set; }
    public string? Note { get; set; }
}

// ─── Status Update ───────────────────────────────────────────────
public class UpdateStatusRequest
{
    public string Status { get; set; } = "";
}

// ─── Settle Request ──────────────────────────────────────────────
public class SettleTicketRequest
{
    public int RiceTypeId { get; set; }
    public bool IsException { get; set; }
    public string? ExceptionReason { get; set; }
    public string? Note { get; set; }
}

// ─── Query Filter ────────────────────────────────────────────────
public class TicketQueryFilter
{
    public string? Status { get; set; }
    public string? DateFrom { get; set; }
    public string? DateTo { get; set; }
    public bool? IsException { get; set; }
}
