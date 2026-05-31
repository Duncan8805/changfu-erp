using System.ComponentModel.DataAnnotations;

namespace ChangFuPOS.Models;

public class PriceLog
{
    public int Id { get; set; }

    public int RiceTypeId { get; set; }
    public RiceType RiceType { get; set; } = null!;

    public DateOnly PriceDate { get; set; }   // 哪一天的牌價

    public decimal UnitPrice { get; set; }    // 元/台斤

    [MaxLength(50)]
    public string CreatedBy { get; set; } = "";
}
