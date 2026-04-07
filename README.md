# OrgUnitAPI

REST API for company organizational structure built with `.NET 8`, `C#`, and `Microsoft SQL Server`.

## Features
- CRUD for companies (`companies`)
- CRUD for employees (`employees`)
- CRUD for organization units (`org-units`)
- Company organization tree (`GET /api/companies/{id}/org-tree`)
- Validation for required fields and uniqueness (codes, emails)
- Validation that unit leader/director belongs to the same company
- Hierarchy validation: `Division -> Project -> Department`
- Consistent error responses via `ProblemDetails`
- OpenAPI + Scalar docs UI

## Project Structure
- `src` - backend API project
- `database/init.sql` - SQL schema/bootstrap script
- `test/scripts/check_api.sh` - baseline API smoke test
- `test/scripts/mock_data_test.sh` - richer integration test
- `test/teapie/CompanyStructure.http` - manual API requests
- `docs/API_CHECK_GUIDE.md` - full validation guide

## Quick Start (Docker, Recommended)

Requirements:
- Docker Desktop (or Docker Engine) with Compose

1. Optional: configure env values (password/ports):
```bash
cp .env.example .env
```
2. Start everything:
```bash
docker compose up --build -d
```
3. Verify containers are up:
```bash
docker compose ps
```
4. Open API:
- Base URL: `http://localhost:18080/api`
- OpenAPI JSON: `http://localhost:18080/openapi/v1.json`
- Scalar UI: `http://localhost:18080/scalar`

Notes:
- Database schema is initialized automatically from `database/init.sql`.
- SQL Server is reachable inside compose network as `db:1433`.

## Run API Tests

Baseline checks:
```bash
BASE_URL=http://localhost:18080/api bash test/scripts/check_api.sh
```

Extended mock-data checks:
```bash
BASE_URL=http://localhost:18080/api bash test/scripts/mock_data_test.sh
```

## Stop / Reset

Stop containers:
```bash
docker compose down
```

Stop and remove database volume (full reset):
```bash
docker compose down -v --remove-orphans
```

## Local Run (Without Docker)

Requirements:
- .NET SDK 8.0+
- SQL Server (Express or full)

1. Create database schema:
```bash
sqlcmd -S localhost\\SQLEXPRESS -i database/init.sql
```
2. Restore and run:
```bash
dotnet restore src/OrgUnitAPi/OrgUnitAPI.csproj
dotnet run --project src/OrgUnitAPi/OrgUnitAPI.csproj
```

Default local endpoints (Development):
- `http://localhost:5132`
- `https://localhost:7245`

## Troubleshooting

If `18080` is already used:
```bash
API_PORT=18081 docker compose up --build -d
```

If startup fails and you want a clean retry:
```bash
docker compose down -v --remove-orphans
docker compose up --build -d
```

Check logs:
```bash
docker compose logs --tail 200 api
docker compose logs --tail 200 db
```
