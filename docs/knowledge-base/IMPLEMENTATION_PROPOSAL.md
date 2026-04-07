# Implementačný návrh – REST API pre organizačnú štruktúru firmy

## 1. Cieľ riešenia
Cieľom je vytvoriť REST API v .NET (C#), ktoré umožní:
- správu firiem,
- správu 4-úrovňovej organizačnej štruktúry (`firma -> divízia -> projekt -> oddelenie`),
- evidenciu zamestnancov,
- priradenie vedúceho ku každému uzlu štruktúry,
- základné validácie a konzistentné chybové odpovede.

## 2. Navrhovaný technologický stack
- .NET 8 (ASP.NET Core Web API)
- C#
- Entity Framework Core + SQL Server provider
- Microsoft SQL Server Express
- Scalar (OpenAPI UI)
- TeaPie (kolekcia requestov na test endpointov)
- xUnit + FluentAssertions (testy)

## 3. Architektúra projektu
Odporúčam jednoduchý modulárny layering (bez zbytočnej komplexity):
- `Api` – controllery, DTO, konfigurácia OpenAPI/Scalar, global exception handling
- `Application` – use-case služby, validačné pravidlá, mapovanie
- `Infrastructure` – EF Core, SQL implementácia repozitárov, migrácie
- `Domain` – entity, enumy, biznis pravidlá (minimálne invariants)

Výhody:
- jasné oddelenie API a business logiky,
- ľahšie testovanie,
- jednoduché rozšírenie bez veľkého refaktoringu.

## 4. Návrh databázy (SQL Server)

### 4.1 Tabuľky
1. `Companies`
- reprezentuje firmu (vrchná úroveň)
- obsahuje kód, názov a voliteľne riaditeľa

2. `Employees`
- zamestnanci firmy
- každý zamestnanec patrí do presne jednej firmy
- evidované polia: titul, meno, priezvisko, telefón, e-mail

3. `OrgUnits`
- organizačné uzly pod firmou
- typ uzla: `Division`, `Project`, `Department`
- self-reference `ParentOrgUnitId` pre strom
- každý uzol má kód, názov a voliteľne vedúceho

4. `EmployeeOrgUnitAssignments` (voliteľné, ale odporúčané)
- väzba zamestnanec -> oddelenie/projekt/divízia
- užitočné pre reporty a budúce rozšírenie

### 4.2 Kľúčové pravidlá konzistencie
- Kód firmy je globálne unikátny.
- Kód organizačného uzla je unikátny v rámci firmy.
- E-mail zamestnanca je unikátny v rámci firmy.
- Vedúci uzla (`LeaderEmployeeId`) musí byť zamestnanec tej istej firmy.
- Povolená hierarchia je striktne:
  - `Division` bez parenta,
  - `Project` musí mať parent typu `Division`,
  - `Department` musí mať parent typu `Project`.
- Mazanie uzla so závislosťami je blokované (`NO ACTION`), aby sa neporušila štruktúra.

### 4.3 Indexy
- `IX_Employees_CompanyId`
- `IX_Employees_CompanyId_Email` (UNIQUE)
- `IX_OrgUnits_CompanyId`
- `IX_OrgUnits_CompanyId_ParentOrgUnitId`
- `IX_OrgUnits_CompanyId_Code` (UNIQUE)

## 5. Návrh API endpointov

### 5.1 Companies
- `POST /api/companies`
- `GET /api/companies`
- `GET /api/companies/{id}`
- `PUT /api/companies/{id}`
- `DELETE /api/companies/{id}`
- `GET /api/companies/{id}/org-tree` (strom divízií/projektov/oddelení)

### 5.2 Employees
- `POST /api/companies/{companyId}/employees`
- `GET /api/companies/{companyId}/employees`
- `GET /api/employees/{id}`
- `PUT /api/employees/{id}`
- `DELETE /api/employees/{id}`

### 5.3 Organization Units
- `POST /api/companies/{companyId}/org-units`
- `GET /api/companies/{companyId}/org-units`
- `GET /api/org-units/{id}`
- `PUT /api/org-units/{id}`
- `DELETE /api/org-units/{id}`

## 6. Validácie a error handling

### 6.1 Validácie vstupov
- `Code`, `Name`, `FirstName`, `LastName`, `Email` sú povinné.
- Kontrola formátu e-mailu.
- Kontrola dĺžok polí (napr. `Code` max 30, `Name` max 200).
- Kontrola povolenej kombinácie parent/typ pre `OrgUnits`.

### 6.2 Chybové odpovede
Odporúčam jednotný formát `ProblemDetails`:
- `400` nevalidné vstupy
- `404` zdroj neexistuje
- `409` konflikt (duplicitný kód/e-mail)
- `422` business pravidlo porušené (napr. zlý parent pre typ uzla)

## 7. Implementačný postup

1. Vytvoriť solution + projekty (`Api`, `Application`, `Infrastructure`, `Domain`, `Tests`).
2. Definovať entity, enumy a DTO kontrakty.
3. Implementovať `DbContext` + fluent konfiguráciu + migrácie.
4. Doplniť SQL bootstrap skript (`database/init.sql`) pre rýchly štart.
5. Implementovať service vrstvu a controllery.
6. Zapnúť OpenAPI + Scalar UI.
7. Pripraviť TeaPie collection s happy-path a error scenármi.
8. Doplniť unit/integration testy.
9. Pridať README s krokmi spustenia.

## 8. Definition of Done
- API pokrýva všetky CRUD požiadavky zo zadania.
- Dodržaná je štruktúra max 4 úrovní.
- Vedúci uzla je validovaný voči firme.
- Databáza je vytvoriteľná cez SQL skript.
- Endpointy sú dostupné cez Scalar.
- TeaPie test kolekcia je súčasťou repozitára.
- README obsahuje presný postup spustenia.

## 9. Poznámka k implementácii hierarchie
Použitie jednej tabuľky `OrgUnits` pre `Division/Project/Department` je zámerné:
- znižuje duplicitu,
- zjednodušuje CRUD endpointy,
- umožňuje jednoduché načítanie stromu pre firmu.

Pravidlá hierarchie sú vynútené kombináciou:
- aplikačnej validácie,
- DB triggera pre ochranu dátovej integrity aj mimo API.

## 10. Analýza databázového návrhu pre tento typ softvéru

### 10.1 Prečo je dátový model kľúčový
Pri systéme organizačnej štruktúry je databáza jadro celej aplikácie. Najčastejšie problémy vznikajú pri:
- nekonzistentných parent-child väzbách,
- nejasnom vlastníctve zamestnancov medzi firmami,
- mazaní dát, ktoré sú ešte referencované,
- zlom výkone pri načítavaní stromu.

Preto je cieľ návrhu nielen "fungovať", ale hlavne udržať **integritu dát** aj pri chybných vstupoch.

### 10.2 Alternatívy modelovania hierarchie

1. Jedna tabuľka pre všetky uzly (`OrgUnits` + `UnitType`)
- plusy: menej duplicity, jednoduchšie API, flexibilita
- mínusy: treba extra validáciu parent/typ (DB trigger + aplikačné pravidlá)

2. Samostatné tabuľky (`Divisions`, `Projects`, `Departments`)
- plusy: silnejšie vynútenie pravidiel cez FK bez triggerov
- mínusy: viac CRUD kódu, zložitejšie reportovanie cez UNION

Pre toto zadanie je vhodnejšia 1. možnosť, lebo API je prehľadnejšie a rýchlejšie implementovateľné.

### 10.3 Integrita vs. flexibilita
Tento typ systému musí preferovať integritu:
- unikátne kódy a e-maily,
- líder z rovnakej firmy,
- povolená hĺbka/typ parenta,
- blokované mazanie referencovaných uzlov.

Flexibilita (napr. ľubovoľná hĺbka stromu) je možná, ale pre toto zadanie by len zvýšila riziko chýb.

### 10.4 Výkonové aspekty
Bežné query vzory:
- list zamestnancov firmy,
- strom organizačných uzlov firmy,
- detail uzla s vedúcim.

Preto sú najdôležitejšie indexy:
- `(CompanyId, Code)` na `OrgUnits`,
- `(CompanyId, Email)` na `Employees`,
- `(CompanyId, ParentOrgUnitId)` na stromové dotazy.

Ak neskôr pribudnú veľké dáta, je vhodné zvážiť:
- cache pre `org-tree` endpoint,
- materializovaný path stĺpec alebo closure table,
- stránkovanie a filtrovanie zamestnancov.

### 10.5 Audit a história
V reálnych HR/organizačných systémoch sa často vyžaduje história zmien:
- kto menil vedúceho,
- kedy zamestnanec prešiel medzi oddeleniami,
- historický pohľad na štruktúru.

Pre produkciu odporúčam doplniť:
- audit tabuľky (`CreatedBy`, `UpdatedBy`, `ChangeReason`),
- temporal tables v SQL Serveri,
- explicitnú históriu priradení zamestnanca (`EmployeeOrgUnitAssignments`).

### 10.6 Riziká a mitigácie
- Riziko: obídenie API priamym SQL insertom.
  Mitigácia: DB constraints + trigger (nie len app validácie).
- Riziko: mazanie vedúceho spôsobí nekonzistentný uzol.
  Mitigácia: FK `NO ACTION` + pravidlo na reassign pred delete.
- Riziko: deadlock pri hromadných updateoch stromu.
  Mitigácia: transakcie, krátke jednotky práce, konzistentné poradie update operácií.

### 10.7 Zhrnutie návrhu
Pre dané zadanie je optimálny kompromis:
- 1 univerzálna tabuľka pre organizačné uzly,
- silné obmedzenia na úrovni DB,
- business validácie v service vrstve,
- pripravenosť na budúce rozšírenia (audit, história, škálovanie).
