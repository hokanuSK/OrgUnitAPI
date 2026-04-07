using CompanyStructure.Api.Application.Common;
using CompanyStructure.Api.Contracts.Companies;
using CompanyStructure.Api.Contracts.OrgUnits;
using CompanyStructure.Api.Domain.Entities;
using CompanyStructure.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CompanyStructure.Api.Application.Services;

public sealed class CompanyService(AppDbContext dbContext)
{
    public async Task<CompanyResponse> CreateAsync(CreateCompanyRequest request, CancellationToken cancellationToken)
    {
        var code = NormalizeCode(request.Code);
        var name = NormalizeText(request.Name);

        if (request.DirectorEmployeeId.HasValue)
        {
            throw AppException.Unprocessable(
                "Director cannot be assigned during company creation. Create the employee first and then update the company.",
                "director_requires_existing_company_employee");
        }

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
        {
            throw AppException.BadRequest("Code and Name are required.", "company_validation");
        }

        var codeExists = await dbContext.Companies
            .AnyAsync(x => x.Code == code, cancellationToken);

        if (codeExists)
        {
            throw AppException.Conflict($"Company code '{code}' already exists.", "company_code_exists");
        }

        var company = new Company
        {
            Code = code,
            Name = name
        };

        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(company);
    }

    public async Task<IReadOnlyCollection<CompanyResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Companies
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<CompanyResponse> GetByIdAsync(int companyId, CancellationToken cancellationToken)
    {
        var company = await dbContext.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CompanyId == companyId, cancellationToken);

        if (company is null)
        {
            throw AppException.NotFound($"Company {companyId} was not found.", "company_not_found");
        }

        return Map(company);
    }

    public async Task<CompanyResponse> UpdateAsync(int companyId, UpdateCompanyRequest request, CancellationToken cancellationToken)
    {
        var company = await dbContext.Companies
            .FirstOrDefaultAsync(x => x.CompanyId == companyId, cancellationToken);

        if (company is null)
        {
            throw AppException.NotFound($"Company {companyId} was not found.", "company_not_found");
        }

        var code = NormalizeCode(request.Code);
        var name = NormalizeText(request.Name);

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
        {
            throw AppException.BadRequest("Code and Name are required.", "company_validation");
        }

        var codeExists = await dbContext.Companies
            .AnyAsync(x => x.CompanyId != companyId && x.Code == code, cancellationToken);

        if (codeExists)
        {
            throw AppException.Conflict($"Company code '{code}' already exists.", "company_code_exists");
        }

        if (request.DirectorEmployeeId.HasValue)
        {
            var directorExists = await dbContext.Employees
                .AnyAsync(x => x.CompanyId == companyId && x.EmployeeId == request.DirectorEmployeeId.Value, cancellationToken);

            if (!directorExists)
            {
                throw AppException.Unprocessable("Director must be an employee of the same company.", "director_company_mismatch");
            }
        }

        company.Code = code;
        company.Name = name;
        company.DirectorEmployeeId = request.DirectorEmployeeId;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(company);
    }

    public async Task DeleteAsync(int companyId, CancellationToken cancellationToken)
    {
        var company = await dbContext.Companies
            .FirstOrDefaultAsync(x => x.CompanyId == companyId, cancellationToken);

        if (company is null)
        {
            throw AppException.NotFound($"Company {companyId} was not found.", "company_not_found");
        }

        var hasEmployees = await dbContext.Employees
            .AnyAsync(x => x.CompanyId == companyId, cancellationToken);

        if (hasEmployees)
        {
            throw AppException.Conflict("Cannot delete company that still contains employees.", "company_has_employees");
        }

        var hasOrgUnits = await dbContext.OrgUnits
            .AnyAsync(x => x.CompanyId == companyId, cancellationToken);

        if (hasOrgUnits)
        {
            throw AppException.Conflict("Cannot delete company that still contains organization units.", "company_has_org_units");
        }

        dbContext.Companies.Remove(company);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<CompanyOrgTreeResponse> GetOrgTreeAsync(int companyId, CancellationToken cancellationToken)
    {
        var company = await dbContext.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CompanyId == companyId, cancellationToken);

        if (company is null)
        {
            throw AppException.NotFound($"Company {companyId} was not found.", "company_not_found");
        }

        var rows = await dbContext.OrgUnits
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId)
            .Select(x => new
            {
                x.OrgUnitId,
                x.ParentOrgUnitId,
                x.UnitType,
                x.Code,
                x.Name,
                x.LeaderEmployeeId,
                LeaderFullName = x.LeaderEmployee == null
                    ? null
                    : x.LeaderEmployee.FirstName + " " + x.LeaderEmployee.LastName
            })
            .ToListAsync(cancellationToken);

        var nodesById = rows.ToDictionary(
            x => x.OrgUnitId,
            x => new OrgUnitTreeNodeResponse
            {
                OrgUnitId = x.OrgUnitId,
                UnitType = x.UnitType,
                Code = x.Code,
                Name = x.Name,
                LeaderEmployeeId = x.LeaderEmployeeId,
                LeaderFullName = x.LeaderFullName
            });

        var roots = new List<OrgUnitTreeNodeResponse>();

        foreach (var row in rows.OrderBy(x => x.UnitType).ThenBy(x => x.Name))
        {
            var current = nodesById[row.OrgUnitId];

            if (row.ParentOrgUnitId is null)
            {
                roots.Add(current);
                continue;
            }

            if (nodesById.TryGetValue(row.ParentOrgUnitId.Value, out var parent))
            {
                parent.Children.Add(current);
            }
        }

        return new CompanyOrgTreeResponse
        {
            CompanyId = company.CompanyId,
            CompanyName = company.Name,
            Divisions = roots
        };
    }

    private static CompanyResponse Map(Company company)
        => new()
        {
            CompanyId = company.CompanyId,
            Code = company.Code,
            Name = company.Name,
            DirectorEmployeeId = company.DirectorEmployeeId
        };

    private static string NormalizeCode(string value)
        => value.Trim().ToUpperInvariant();

    private static string NormalizeText(string value)
        => value.Trim();
}
