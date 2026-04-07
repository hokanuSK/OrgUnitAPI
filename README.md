# OrgUnitAPI - REST API (Organizačná štruktúra firmy)

Implementácia REST API v `.NET 8` + `C#` + `SQL Server`.

## Čo je implementované
- CRUD pre firmy (`companies`)
- CRUD pre zamestnancov (`employees`)
- CRUD pre organizačné uzly (`org-units`)
- strom organizácie firmy (`GET /api/companies/{id}/org-tree`)
- validácie povinných polí
- validácie unikátnych kódov/e-mailov
- validácia, že líder uzla je zamestnanec rovnakej firmy
- validácia povolenej hierarchie: `Division -> Project -> Department`
- jednotný error formát cez `ProblemDetails`
- OpenAPI + Scalar UI

## Štruktúra projektu
- `src` - API projekt
- `database/init.sql` - SQL skript na vytvorenie databázy
- `teapie/CompanyStructure.http` - requesty pre TeaPie testovanie
- `docs/zadanie.md` - zadanie
- `docs/knowledge-base/IMPLEMENTATION_PROPOSAL.md` - implementačný návrh
- `docs/knowledge-base/DATABASE_ANALYSIS.md` - databázová analýza
- `docs/API_CHECK_GUIDE.md` - kompletný návod na overenie API

## Predpoklady
- .NET SDK 8.0+
- SQL Server Express (alebo iný SQL Server)

## Spustenie
1. Vytvor databázu spustením SQL skriptu `database/init.sql`.
2. Skontroluj connection string v `src/appsettings.json` a `src/appsettings.Development.json`.
3. Obnov balíčky:
```bash
dotnet restore src/OrgUnitAPI.csproj
```
4. Spusti API:
```bash
dotnet run --project src/OrgUnitAPI.csproj
```

## Scalar a OpenAPI
Po štarte API (Development profil):
- OpenAPI JSON: `https://localhost:7245/openapi/v1.json`
- Scalar UI: `https://localhost:7245/scalar`

## TeaPie testovanie
Použi súbor:
- `teapie/CompanyStructure.http`

Odporúčaný postup requestov:
1. Create company
2. Create employee
3. Update company (nastaviť director)
4. Create division
5. Create project
6. Create department
7. Get org-tree

## Poznámka
Tento workspace nemá nainštalovaný `dotnet` SDK, preto build/run nebol vykonaný lokálne v tomto prostredí. Zdrojové kódy sú pripravené na spustenie v prostredí s .NET 8 a SQL Server.

## Automatické overenie
- Základný API check:
```bash
BASE_URL=http://localhost:5132/api bash scripts/check_api.sh
```
- Rozšírený mock-data integračný test:
```bash
BASE_URL=http://localhost:5132/api bash scripts/mock_data_test.sh
```

Pri Docker run-e z tohto overenia použi `BASE_URL=http://localhost:18080/api`.
