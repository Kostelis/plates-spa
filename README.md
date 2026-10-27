# Plates

SaaS for gyms and personal coaches: a coach assigns strength training programs to clients, and after each logged workout the progression engine calculates the next session's weight and reps.

## Projects

| Project | Purpose |
|---|---|
| `src/Plates.Engine` | Progression rules. Class library with no NuGet packages. |
| `src/Plates.Api` | ASP.NET Core Web API (PostgreSQL). Publishes an OpenAPI document in Development. |
| `tests/Plates.Engine.Tests` | xUnit unit tests for the engine. |
| `tests/Plates.Api.Tests` | xUnit integration tests for the API. |

## Prerequisites

- .NET 10 SDK
- Docker with Compose

## Run

```sh
cp .env.example .env    # then set POSTGRES_PASSWORD in .env
docker compose up --build
```

This starts PostgreSQL on port 5432 and the API on http://localhost:8080. The OpenAPI document is at http://localhost:8080/openapi/v1.json.

To run the API outside Docker: `dotnet run --project src/Plates.Api`.

## Demo

Open http://localhost:8080. The page starts with the section 7 earlier workout already run through the engine; its state lives in the page only, so a reload starts over.

1. The state cards show squat at 60 kg (Record 74 kg) and bench press at 47,5 kg with Failure counter 1 (Record 58,6 kg).
2. Keep squat at 60 × 8, 8, 8, change bench press reps to 7, 5, 4, and press „Registruoti treniruotę“.
3. Squat goes to 65 kg (DIDINTI, Record 76 kg) and bench press to 42,5 kg (MAŽINTI, Failure counter 0); the preset buttons show the rule 1 errors and the 500 kg limit.

## Build and test

```sh
dotnet build
dotnet test
```

Tests run on Microsoft.Testing.Platform (set in `global.json`).
