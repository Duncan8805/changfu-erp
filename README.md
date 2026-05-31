# 長富稻穀廠 POS 系統

地磅傳票數位化系統，供農民繳穀時即時過磅、計算金額、列印傳票。

## Tech Stack

- **Frontend**: Vue 3 + Vite + Tailwind CSS + Pinia
- **Backend**: ASP.NET Core 8 Web API + EF Core 8
- **Database**: Aiven MySQL (production) / SQLite (development)

## 本機開發

```bash
# 後端
cd backend
dotnet run

# 前端
cd frontend
npm install
npm run dev
```

## 部署

- **Frontend**: GitHub Pages (`https://Duncan8805.github.io/changfu-erp/`)
- **Backend**: Render.com (Docker)
- **Database**: Aiven MySQL
