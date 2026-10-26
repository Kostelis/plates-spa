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

## Build and test

```sh
dotnet build
dotnet test
```

Tests run on Microsoft.Testing.Platform (set in `global.json`).
