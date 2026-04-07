using CompanyStructure.Api.Application.Common;
using CompanyStructure.Api.Contracts.OrgUnits;
using CompanyStructure.Api.Domain.Entities;
using CompanyStructure.Api.Domain.Enums;
using CompanyStructure.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CompanyStructure.Api.Application.Services;

public sealed class OrgUnitService(AppDbContext dbContext)
{
    public async Task<OrgUnitResponse> CreateAsync(int companyId, CreateOrgUnitRequest request, CancellationToken cancellationToken)
    {
        await EnsureCompanyExists(companyId, cancellationToken);

        var code = NormalizeCode(request.Code);
        var name = NormalizeText(request.Name);

        await EnsureCodeUnique(companyId, code, null, cancellationToken);
        await EnsureLeaderIsValid(companyId, request.LeaderEmployeeId, cancellationToken);
        await ValidateHierarchy(companyId, request.UnitType, request.ParentOrgUnitId, null, cancellationToken);

        var orgUnit = new OrgUnit
        {
            CompanyId = companyId,
            UnitType = request.UnitType,
            ParentOrgUnitId = request.ParentOrgUnitId,
            Code = code,
            Name = name,
            LeaderEmployeeId = request.LeaderEmployeeId
        };

        dbContext.OrgUnits.Add(orgUnit);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(orgUnit);
    }

    public async Task<IReadOnlyCollection<OrgUnitResponse>> GetByCompanyAsync(int companyId, CancellationToken cancellationToken)
    {
        await EnsureCompanyExists(companyId, cancellationToken);

        return await dbContext.OrgUnits
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId)
            .OrderBy(x => x.UnitType)
            .ThenBy(x => x.Name)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<OrgUnitResponse> GetByIdAsync(int orgUnitId, CancellationToken cancellationToken)
    {
        var orgUnit = await dbContext.OrgUnits
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrgUnitId == orgUnitId, cancellationToken);

        if (orgUnit is null)
        {
            throw AppException.NotFound($"Org unit {orgUnitId} was not found.", "org_unit_not_found");
        }

        return Map(orgUnit);
    }

    public async Task<OrgUnitResponse> UpdateAsync(int orgUnitId, UpdateOrgUnitRequest request, CancellationToken cancellationToken)
    {
        var orgUnit = await dbContext.OrgUnits
            .FirstOrDefaultAsync(x => x.OrgUnitId == orgUnitId, cancellationToken);

        if (orgUnit is null)
        {
            throw AppException.NotFound($"Org unit {orgUnitId} was not found.", "org_unit_not_found");
        }

        var code = NormalizeCode(request.Code);
        var name = NormalizeText(request.Name);

        await EnsureCodeUnique(orgUnit.CompanyId, code, orgUnitId, cancellationToken);
        await EnsureLeaderIsValid(orgUnit.CompanyId, request.LeaderEmployeeId, cancellationToken);
        await ValidateHierarchy(orgUnit.CompanyId, request.UnitType, request.ParentOrgUnitId, orgUnitId, cancellationToken);

        orgUnit.UnitType = request.UnitType;
        orgUnit.ParentOrgUnitId = request.ParentOrgUnitId;
        orgUnit.Code = code;
        orgUnit.Name = name;
        orgUnit.LeaderEmployeeId = request.LeaderEmployeeId;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(orgUnit);
    }

    public async Task DeleteAsync(int orgUnitId, CancellationToken cancellationToken)
    {
        var orgUnit = await dbContext.OrgUnits
            .FirstOrDefaultAsync(x => x.OrgUnitId == orgUnitId, cancellationToken);

        if (orgUnit is null)
        {
            throw AppException.NotFound($"Org unit {orgUnitId} was not found.", "org_unit_not_found");
        }

        var hasChildren = await dbContext.OrgUnits
            .AnyAsync(x => x.CompanyId == orgUnit.CompanyId && x.ParentOrgUnitId == orgUnitId, cancellationToken);

        if (hasChildren)
        {
            throw AppException.Conflict("Cannot delete organization unit with child units.", "org_unit_has_children");
        }

        var hasAssignments = await dbContext.EmployeeOrgUnitAssignments
            .AnyAsync(x => x.CompanyId == orgUnit.CompanyId && x.OrgUnitId == orgUnitId, cancellationToken);

        if (hasAssignments)
        {
            throw AppException.Conflict("Cannot delete organization unit with employee assignments.", "org_unit_has_assignments");
        }

        dbContext.OrgUnits.Remove(orgUnit);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureCompanyExists(int companyId, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Companies
            .AnyAsync(x => x.CompanyId == companyId, cancellationToken);

        if (!exists)
        {
            throw AppException.NotFound($"Company {companyId} was not found.", "company_not_found");
        }
    }

    private async Task EnsureCodeUnique(int companyId, string code, int? currentOrgUnitId, CancellationToken cancellationToken)
    {
        var exists = await dbContext.OrgUnits
            .AnyAsync(x => x.CompanyId == companyId
                           && x.Code == code
                           && (!currentOrgUnitId.HasValue || x.OrgUnitId != currentOrgUnitId.Value), cancellationToken);

        if (exists)
        {
            throw AppException.Conflict($"Organization unit code '{code}' already exists in company {companyId}.", "org_unit_code_exists");
        }
    }

    private async Task EnsureLeaderIsValid(int companyId, int? leaderEmployeeId, CancellationToken cancellationToken)
    {
        if (!leaderEmployeeId.HasValue)
        {
            return;
        }

        var leaderExists = await dbContext.Employees
            .AnyAsync(x => x.CompanyId == companyId && x.EmployeeId == leaderEmployeeId.Value, cancellationToken);

        if (!leaderExists)
        {
            throw AppException.Unprocessable("Leader must be an employee of the same company.", "leader_company_mismatch");
        }
    }

    private async Task ValidateHierarchy(
        int companyId,
        OrgUnitType unitType,
        int? parentOrgUnitId,
        int? currentOrgUnitId,
        CancellationToken cancellationToken)
    {
        if (currentOrgUnitId.HasValue && parentOrgUnitId == currentOrgUnitId)
        {
            throw AppException.Unprocessable("Organization unit cannot be parent of itself.", "org_unit_self_parent");
        }

        OrgUnit? parent = null;

        if (parentOrgUnitId.HasValue)
        {
            parent = await dbContext.OrgUnits
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.OrgUnitId == parentOrgUnitId.Value, cancellationToken);

            if (parent is null)
            {
                throw AppException.Unprocessable("Parent organization unit does not exist in the same company.", "org_unit_parent_not_found");
            }
        }

        switch (unitType)
        {
            case OrgUnitType.Division:
                if (parentOrgUnitId.HasValue)
                {
                    throw AppException.Unprocessable("Division cannot have a parent.", "invalid_division_parent");
                }

                break;

            case OrgUnitType.Project:
                if (parent is null || parent.UnitType != OrgUnitType.Division)
                {
                    throw AppException.Unprocessable("Project must have parent of type Division.", "invalid_project_parent");
                }

                break;

            case OrgUnitType.Department:
                if (parent is null || parent.UnitType != OrgUnitType.Project)
                {
                    throw AppException.Unprocessable("Department must have parent of type Project.", "invalid_department_parent");
                }

                break;

            default:
                throw AppException.BadRequest("Invalid organization unit type.", "invalid_org_unit_type");
        }

        if (currentOrgUnitId.HasValue)
        {
            await ValidateChildrenCompatibility(companyId, currentOrgUnitId.Value, unitType, cancellationToken);
        }
    }

    private async Task ValidateChildrenCompatibility(int companyId, int currentOrgUnitId, OrgUnitType newType, CancellationToken cancellationToken)
    {
        var childTypes = await dbContext.OrgUnits
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.ParentOrgUnitId == currentOrgUnitId)
            .Select(x => x.UnitType)
            .Distinct()
            .ToListAsync(cancellationToken);

        switch (newType)
        {
            case OrgUnitType.Division:
                if (childTypes.Any(x => x != OrgUnitType.Project))
                {
                    throw AppException.Unprocessable("Division can only contain Project child units.", "invalid_division_children");
                }

                break;

            case OrgUnitType.Project:
                if (childTypes.Any(x => x != OrgUnitType.Department))
                {
                    throw AppException.Unprocessable("Project can only contain Department child units.", "invalid_project_children");
                }

                break;

            case OrgUnitType.Department:
                if (childTypes.Count != 0)
                {
                    throw AppException.Unprocessable("Department cannot contain child units.", "invalid_department_children");
                }

                break;
        }
    }

    private static OrgUnitResponse Map(OrgUnit orgUnit)
        => new()
        {
            OrgUnitId = orgUnit.OrgUnitId,
            CompanyId = orgUnit.CompanyId,
            UnitType = orgUnit.UnitType,
            ParentOrgUnitId = orgUnit.ParentOrgUnitId,
            Code = orgUnit.Code,
            Name = orgUnit.Name,
            LeaderEmployeeId = orgUnit.LeaderEmployeeId
        };

    private static string NormalizeCode(string value)
    {
        var result = value.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(result))
        {
            throw AppException.BadRequest("Code is required.", "org_unit_code_required");
        }

        return result;
    }

    private static string NormalizeText(string value)
    {
        var result = value.Trim();

        if (string.IsNullOrWhiteSpace(result))
        {
            throw AppException.BadRequest("Name is required.", "org_unit_name_required");
        }

        return result;
    }
}
