using Microsoft.EntityFrameworkCore;
using ChangFuPOS.Models;

namespace ChangFuPOS.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<RiceType> RiceTypes => Set<RiceType>();
    public DbSet<PriceLog> PriceLogs => Set<PriceLog>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 複合唯一索引：同一天同一米種只能有一筆牌價
        modelBuilder.Entity<PriceLog>()
            .HasIndex(p => new { p.RiceTypeId, p.PriceDate })
            .IsUnique();

        // Seed Data
        var today = DateOnly.FromDateTime(DateTime.Today);

        modelBuilder.Entity<RiceType>().HasData(
            new RiceType { Id = 1, Name = "蓬萊米 (濕)", IsActive = true },
            new RiceType { Id = 2, Name = "在來米 (濕)", IsActive = true },
            new RiceType { Id = 3, Name = "蓬萊米 (乾)", IsActive = true },
            new RiceType { Id = 4, Name = "在來米 (乾)", IsActive = true }
        );


        // 預設帳號：admin / admin123
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "admin",
                IsActive = true
            },
            new User
            {
                Id = 2,
                Username = "cashier",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("cashier123"),
                Role = "cashier",
                IsActive = true
            }
        );
    }
}
