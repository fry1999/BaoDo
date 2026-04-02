# BaoDo

Nen tang luyen thi TOEIC thong minh.

## Tech Stack
- Frontend: Nuxt 4 + TypeScript + Tailwind CSS
- Backend: ASP.NET Core 8 (Minimal API)
- Database: Supabase (PostgreSQL)
- Cache: Redis (optional, tu dong fallback memory cache)
- AI: OpenAI GPT-4o mini

## Yeu cau moi truong
- Node.js 20+ va npm
- .NET SDK 8.0+
- Tai khoan Supabase (lay Postgres connection string)
- Redis local (khong bat buoc)

## Cau truc thu muc
- `frontend`: ung dung Nuxt
- `backend`: API .NET + business logic
- `supabase/migrations`: SQL migration de tao/cap nhat schema

## 1) Cai dat frontend
```bash
cd frontend
cp .env.example .env
npm install
```

Cap nhat file `frontend/.env`:
- `NUXT_PUBLIC_API_BASE_URL=http://localhost:5000`
- `NUXT_PUBLIC_SUPABASE_URL=...`
- `NUXT_PUBLIC_SUPABASE_ANON_KEY=...`

Chay frontend:
```bash
cd frontend
npm run dev
```

Frontend mac dinh chay tai `http://localhost:3000`.

## 2) Cai dat backend
```bash
cd backend
dotnet restore BaoDo.sln
```

Co 2 cach cau hinh bien moi truong backend:

### Cach A - dung bien moi truong (.env tham khao)
Tham khao file `backend/BaoDo.API/.env.example` va set cac bien:
- `ConnectionStrings__DefaultConnection`
- `ConnectionStrings__Redis` (optional)
- `Jwt__Secret` (toi thieu 32 ky tu)
- `OpenAI__ApiKey`
- `AllowedOrigins`

### Cach B - dung `appsettings.Development.json`
Khai bao gia tri trong `backend/BaoDo.API/appsettings.Development.json`.

> Khuyen nghi: khong commit secret that (DB password, JWT secret, OpenAI key) len git.

Chay backend:
```bash
cd backend
dotnet run --project BaoDo.API
```

Backend se expose API/Swagger o moi truong Development (thuong la `http://localhost:5000/swagger`).

## 3) Khoi tao database (Supabase)
Mo Supabase SQL Editor va chay lan luot cac file:
1. `supabase/migrations/001_init_schema.sql`
2. `supabase/migrations/002_rls_policies.sql`
3. `supabase/migrations/003_enum_columns_to_text.sql`
4. `supabase/migrations/004_ranking_indexes.sql`

## 4) Chay toan bo he thong
Mo 2 terminal:

Terminal 1:
```bash
cd backend
dotnet run --project BaoDo.API
```

Terminal 2:
```bash
cd frontend
npm run dev
```

Truy cap:
- Frontend: `http://localhost:3000`
- Backend Swagger: `http://localhost:5000/swagger`

## Troubleshooting nhanh
- Loi CORS: kiem tra `AllowedOrigins` co `http://localhost:3000`.
- Frontend goi API loi: kiem tra `NUXT_PUBLIC_API_BASE_URL`.
- Loi DB ket noi: kiem tra `ConnectionStrings__DefaultConnection`.
- Redis khong chay: co the bo trong `ConnectionStrings__Redis`, he thong se dung memory cache.

Ctrl + Shift + Esc -> Tab Processes 


netstat -ano | findstr :3000
taskkill /PID 5644 /F
