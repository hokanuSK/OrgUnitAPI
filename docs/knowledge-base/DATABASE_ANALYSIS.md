# Database Analysis for Organizational Structure Projects

## Recommended Core Model
- `Companies` as tenant root (`CompanyId`, unique `Code`, `Name`, `DirectorEmployeeId`).
- `Employees` linked to company (`CompanyId` FK, title, first/last name, phone, email).
- `OrgUnits` as one hierarchy table (`UnitType` = `Division`/`Project`/`Department`, `ParentOrgUnitId`, `LeaderEmployeeId`).
- Optional `EmployeeOrgUnitAssignments` for assignment tracking/history.

## Why This Structure Fits This Project Type
- A single `OrgUnits` table avoids duplicated logic across multiple hierarchy tables.
- API implementation is simpler and more consistent (one CRUD flow for organizational nodes).
- `CompanyId` on all major tables keeps tenant boundaries explicit and enforceable.
- For fixed-depth hierarchies (like max 4 levels), adjacency list (`ParentOrgUnitId`) is usually the best complexity/performance tradeoff.

## Integrity Rules That Matter Most
- Unique company code.
- Unique employee email per company.
- Unique org unit code per company.
- Node leader must belong to the same company.
- Hierarchy must be constrained:
  - `Division` has no parent.
  - `Project` parent must be `Division`.
  - `Department` parent must be `Project`.
- Deletes should be restricted (`NO ACTION`) where references exist to prevent broken hierarchy.

## Performance Considerations
- Most frequent queries in these systems:
  - Employees by company.
  - Full org tree for a company.
  - Node detail with leader data.
- Key indexes:
  - `Employees(CompanyId, Email)`.
  - `OrgUnits(CompanyId, Code)`.
  - `OrgUnits(CompanyId, ParentOrgUnitId)`.
- If data grows significantly:
  - Add caching for full tree endpoints.
  - Consider read-optimized projections or path-based hierarchy support.

## Operational and Enterprise Concerns
- Use `RowVersion` for optimistic concurrency in update endpoints.
- Add audit/history support (temporal tables or audit log tables).
- Prefer soft delete for employees in HR-like systems.
- Keep database constraints/triggers as defense-in-depth in case direct SQL writes bypass API validation.

## Common Risks and Mitigations
- Risk: Invalid hierarchy from manual SQL edits.
  Mitigation: FK constraints + CHECK/trigger hierarchy validation.
- Risk: Inconsistent leader references.
  Mitigation: composite FK ensuring leader belongs to same company.
- Risk: accidental data loss during delete.
  Mitigation: restricted deletes + explicit reassignment flows.

## Final Recommendation
For this class of software, prioritize a schema that is:
- company-scoped,
- strongly constrained at DB level,
- simple to query for tree reads,
- ready for audit/history extension.

This gives a robust baseline for both assignment-level implementation and real-world scaling.
