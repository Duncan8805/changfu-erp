using System.ComponentModel.DataAnnotations;

namespace ChangFuPOS.Models;

public class RiceType
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } = "";

    public bool IsActive { get; set; } = true;

    public ICollection<PriceLog> PriceLogs { get; set; } = new List<PriceLog>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
