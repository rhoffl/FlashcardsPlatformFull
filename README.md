
# FlashcardsPlatformFull (.NET 10 + PostgreSQL + EF Core + Identity)

## Features
- Decks + Flashcards CRUD (Admin-only)
- Study mode with flip animation
- Per-user spaced repetition (SM-2 inspired)
- Leaderboard / gamification stats
- Admin dashboard + generator page (OpenAI stub; add API key to enable)
- Public REST API: /swagger
- Dockerfile + docker-compose (Postgres + web)

## Quick start (local)
1) Configure PostgreSQL connection string in appsettings.json
2) Install EF tool if needed:
   dotnet tool install --global dotnet-ef
3) Create migrations and update DB:
   dotnet ef migrations add InitialCreate
   dotnet ef database update
4) Run:
   dotnet run
5) Visit:
   / (Decks)
   /swagger (API docs)
   /Identity/Account/Login

## Default seeded admin
Email: admin@local
Password: Admin123!ChangeMe

## Docker
docker compose up --build
Then browse http://localhost:8080

## Notes about .NET 10
If your environment doesn't have .NET 10 / images yet, change:
- TargetFramework net10.0 -> net8.0 (csproj)
- Dockerfile images 10.0 -> 8.0
