using System.ComponentModel.DataAnnotations;

namespace ChangFuPOS.Models;

public class User
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string Username { get; set; } = "";

    public string PasswordHash { get; set; } = "";   // BCrypt hash

    // role: "admin" | "cashier"
    [MaxLength(20)]
    public string Role { get; set; } = "cashier";

    public bool IsActive { get; set; } = true;
}
