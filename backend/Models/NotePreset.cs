using System.ComponentModel.DataAnnotations;

namespace ChangFuPOS.Models;

public class NotePreset
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string Content { get; set; } = "";

    public int SortOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
