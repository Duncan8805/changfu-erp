using System.ComponentModel.DataAnnotations;

namespace ChangFuPOS.Models;

public class Ticket
{
    public int Id { get; set; }

    [MaxLength(20)]
    public string TicketNo { get; set; } = "";       // 系統自動產生，格式 No.XXXXXX

    public int RiceTypeId { get; set; }
    public RiceType RiceType { get; set; } = null!;

    public decimal PriceSnapshot { get; set; }       // 結算當下單價快照（不可改）

    [MaxLength(20)]
    public string VehicleNo { get; set; } = "";      // 車號

    [MaxLength(50)]
    public string FarmerName { get; set; } = "";     // 農民姓名

    [MaxLength(50)]
    public string Village { get; set; } = "";        // 村別/行號

    public decimal GrossWeightKg { get; set; }       // 總重
    public decimal TareWeightKg { get; set; }        // 空重
    public decimal NetWeightKg { get; set; }         // 淨重 = 總重 - 空重
    public decimal NetWeightJin { get; set; }        // 台斤 = NetWeightKg / 0.6（存入不靠計算）
    public decimal TotalAmount { get; set; }         // 總金額 = NetWeightJin * PriceSnapshot

    public bool IsException { get; set; } = false;

    [MaxLength(50)]
    public string? ExceptionReason { get; set; }     // 太青/含水高/雜質/蟲害/摻沙/其他

    [MaxLength(200)]
    public string? Note { get; set; }

    // status: "unloading" | "pending" | "settled"
    [MaxLength(20)]
    public string Status { get; set; } = "unloading";

    public DateTime? SettledAt { get; set; }

    [MaxLength(50)]
    public string CreatedBy { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
