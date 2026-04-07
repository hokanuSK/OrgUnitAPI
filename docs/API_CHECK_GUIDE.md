# API Check Guide (OrgUnitAPI)

## 1. Scope (what must be validated)
Based on `docs/zadanie.md`, verify:
- CRUD behavior for companies, employees, and org units.
- Hierarchy behavior: `Company -> Division -> Project -> Department`.
- Leader and director must belong to the same company.
- Validation and error codes for invalid inputs.
- Data persistence in SQL Server.
- OpenAPI JSON and Scalar UI availability.

## 2. Current Project Paths
- Solution: `OrgUnitAPI.sln`
- API project: `src/OrgUnitAPi/OrgUnitAPI.csproj`
- SQL schema: `database/init.sql`
- TeaPie requests: `test/teapie/CompanyStructure.http`
- Baseline check script: `test/scripts/check_api.sh`
- Extended mock-data script: `test/scripts/mock_data_test.sh`

## 3. Run API
### Option A: local .NET + local SQL Server
1. Create database schema:
```bash
sqlcmd -S localhost\\SQLEXPRESS -i database/init.sql
```
2. Start API:
```bash
dotnet restore src/OrgUnitAPi/OrgUnitAPI.csproj
dotnet run --project src/OrgUnitAPi/OrgUnitAPI.csproj
```
3. API docs:
- OpenAPI JSON: `https://localhost:7245/openapi/v1.json`
- Scalar: `https://localhost:7245/scalar`

### Option B: Docker (used in live verification)
1. (Optional) configure env:
```bash
cp .env.example .env
```
2. Start stack:
```bash
docker compose up --build -d
```
3. Check state:
```bash
docker compose ps
```
4. API base URL used: `http://localhost:18080/api`
5. If `18080` is occupied:
```bash
API_PORT=18081 docker compose up --build -d
```

## 4. Baseline API Verification
Run:
```bash
BASE_URL=http://localhost:18080/api bash test/scripts/check_api.sh
```

Expected baseline statuses:
1. `POST /api/companies` -> `201`
2. `POST /api/companies/{companyId}/employees` -> `201`
3. `PUT /api/companies/{companyId}` (set director) -> `200`
4. Invalid project without parent -> `422`
5. Valid division/project/department creation -> `201`
6. `GET /api/companies/{companyId}/org-tree` -> `200`
7. Missing email validation -> `400`
8. Duplicate email validation -> `409`

## 5. Rich Mock-Data Regression Test
Run:
```bash
BASE_URL=http://localhost:18080/api bash test/scripts/mock_data_test.sh
```

This test validates:
- Two companies with isolated data.
- Employee and org-unit list counts per company.
- Tree shape checks for both companies.
- Duplicate company code rejection (`409`).
- Duplicate org-unit code rejection in same company (`409`).
- Invalid hierarchy update rejection (`422`).
- Cross-company leader/director rejection (`422`).
- Parent delete protection (`409`).
- Valid delete sequence leaf -> project -> division (`204`).
- Director delete protection (`409`).
- Company delete protection while employees exist (`409`).

## 6. Back-Check Steps
After scripts pass, verify:
1. OpenAPI endpoint:
```bash
curl -sS -o /tmp/openapi.json -w '%{http_code}\n' http://localhost:18080/openapi/v1.json
jq -r '.info.title' /tmp/openapi.json
```
2. Scalar endpoint:
```bash
curl -sSL -o /tmp/scalar.html -w '%{http_code}\n' http://localhost:18080/scalar
```
3. Runtime logs (no exceptions):
```bash
docker compose logs --tail 300 api 2>&1 | rg -n "fail|exception|crit|Unhandled|500" -i
```

## 7. Cleanup Commands
Stop:
```bash
docker compose down
```

Full reset (including DB volume):
```bash
docker compose down -v --remove-orphans
```

## 8. Live Verification Snapshot
Verified on **April 7, 2026**.
- Baseline script: passed.
- Extended mock-data script: passed.
- OpenAPI `/openapi/v1.json`: `200`.
- Scalar `/scalar` (follow redirects): `200`.
- API log scan: no exception/error matches.

## 9. Pass Criteria
Mark the API as verified when:
- Baseline script passes.
- Extended mock-data script passes.
- OpenAPI and Scalar are reachable.
- No runtime exceptions appear during the test window.
