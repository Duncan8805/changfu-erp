using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ChangFuPOS.Data;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
var isMySQL = !connectionString.StartsWith("Data Source", StringComparison.OrdinalIgnoreCase);

if (isMySQL)
{
    // 正式環境：MySQL (Aiven)
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(connectionString,
            new MySqlServerVersion(new Version(8, 0, 28)),
            mySqlOptions => mySqlOptions.EnableRetryOnFailure()));
}
else
{
    // 開發環境：SQLite
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString));
}

// ─── JWT Authentication ───────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ─── CORS ──────────────────────────────────────────────────────────
var allowedOrigins = new[]
{
    "http://localhost:5173",
    "https://Duncan8805.github.io",
    // 從環境變數讀取額外允許的 Origin（Render 部署時可追加）
    builder.Configuration["AllowedOrigin"] ?? "",
}.Where(o => !string.IsNullOrEmpty(o)).ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ─── Controllers + JSON ───────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        // camelCase JSON
        opts.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ─── Build ────────────────────────────────────────────────────────
var app = builder.Build();

// Auto-migrate + seed on startup（含 retry，防止 MySQL 短暫不可用時 crash）
var retries = 0;
const int maxRetries = 5;
while (retries < maxRetries)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();

        // Seed 今日牌價（第一次啟動時）
        var today = DateOnly.FromDateTime(DateTime.Today);
        var defaultPrices = new[]
        {
            new { RiceTypeId = 1, Price = 12.5m },
            new { RiceTypeId = 2, Price = 13.2m },
            new { RiceTypeId = 3, Price = 15.0m },
            new { RiceTypeId = 4, Price = 16.8m },
        };
        foreach (var p in defaultPrices)
        {
            var exists = db.PriceLogs.Any(pl => pl.RiceTypeId == p.RiceTypeId && pl.PriceDate == today);
            if (!exists)
            {
                db.PriceLogs.Add(new ChangFuPOS.Models.PriceLog
                {
                    RiceTypeId = p.RiceTypeId,
                    PriceDate  = today,
                    UnitPrice  = p.Price,
                    CreatedBy  = "system"
                });
            }
        }
        db.SaveChanges();
        break; // 成功，跳出 retry loop
    }
    catch (Exception ex)
    {
        retries++;
        Console.WriteLine($"[Startup] DB 連線失敗（第 {retries}/{maxRetries} 次）：{ex.Message}");
        if (retries >= maxRetries)
        {
            Console.WriteLine("[Startup] 超過最大重試次數，繼續啟動（DB 功能暫時不可用）");
            break;
        }
        Thread.Sleep(TimeSpan.FromSeconds(Math.Pow(2, retries))); // 2s, 4s, 8s, 16s, 32s
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendDev");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 開發環境監聽 5000
app.Run();
