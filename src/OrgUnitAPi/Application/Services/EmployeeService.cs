using CompanyStructure.Api.Application.Common;
using CompanyStructure.Api.Contracts.Employees;
using CompanyStructure.Api.Domain.Entities;
using CompanyStructure.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CompanyStructure.Api.Application.Services;

public sealed class EmployeeService(AppDbContext dbContext) : IEmployeeService
{
    public async Task<EmployeeResponse> CreateAsync(int companyId, CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        await EnsureCompanyExists(companyId, cancellationToken);

        var email = NormalizeEmail(request.Email);

        var emailExists = await dbContext.Employees
            .AnyAsync(x => x.CompanyId == companyId && x.Email.ToLower() == email, cancellationToken);

        if (emailExists)
        {
            throw AppException.Conflict($"Employee email '{email}' already exists in company {companyId}.", "employee_email_exists");
        }

        var employee = new Employee
        {
            CompanyId = companyId,
            Title = NormalizeNullable(request.Title),
            FirstName = NormalizeText(request.FirstName),
            LastName = NormalizeText(request.LastName),
            Phone = NormalizeNullable(request.Phone),
            Email = email,
            IsActive = true
        };

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(employee);
    }

    public async Task<IReadOnlyCollection<EmployeeResponse>> GetByCompanyAsync(int companyId, CancellationToken cancellationToken)
    {
        await EnsureCompanyExists(companyId, cancellationToken);

        return await dbContext.Employees
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId)
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployeeResponse> GetByIdAsync(int employeeId, CancellationToken cancellationToken)
    {
        var employee = await dbContext.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId, cancellationToken);

        if (employee is null)
        {
            throw AppException.NotFound($"Employee {employeeId} was not found.", "employee_not_found");
        }

        return Map(employee);
    }

    public async Task<EmployeeResponse> UpdateAsync(int employeeId, UpdateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var employee = await dbContext.Employees
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId, cancellationToken);

        if (employee is null)
        {
            throw AppException.NotFound($"Employee {employeeId} was not found.", "employee_not_found");
        }

        var email = NormalizeEmail(request.Email);

        var emailExists = await dbContext.Employees
            .AnyAsync(x => x.CompanyId == employee.CompanyId && x.EmployeeId != employeeId && x.Email.ToLower() == email, cancellationToken);

        if (emailExists)
        {
            throw AppException.Conflict($"Employee email '{email}' already exists in company {employee.CompanyId}.", "employee_email_exists");
        }

        employee.Title = NormalizeNullable(request.Title);
        employee.FirstName = NormalizeText(request.FirstName);
        employee.LastName = NormalizeText(request.LastName);
        employee.Phone = NormalizeNullable(request.Phone);
        employee.Email = email;
        employee.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(employee);
    }

    public async Task DeleteAsync(int employeeId, CancellationToken cancellationToken)
    {
        var employee = await dbContext.Employees
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId, cancellationToken);

        if (employee is null)
        {
            throw AppException.NotFound($"Employee {employeeId} was not found.", "employee_not_found");
        }

        var isDirector = await dbContext.Companies
            .AnyAsync(x => x.CompanyId == employee.CompanyId && x.DirectorEmployeeId == employeeId, cancellationToken);

        if (isDirector)
        {
            throw AppException.Conflict("Cannot delete employee who is assigned as company director.", "employee_is_director");
        }

        var isOrgUnitLeader = await dbContext.OrgUnits
            .AnyAsync(x => x.CompanyId == employee.CompanyId && x.LeaderEmployeeId == employeeId, cancellationToken);

        if (isOrgUnitLeader)
        {
            throw AppException.Conflict("Cannot delete employee who is assigned as organization unit leader.", "employee_is_leader");
        }

        var hasAssignments = await dbContext.EmployeeOrgUnitAssignments
            .AnyAsync(x => x.CompanyId == employee.CompanyId && x.EmployeeId == employeeId, cancellationToken);

        if (hasAssignments)
        {
            throw AppException.Conflict("Cannot delete employee with organization unit assignments.", "employee_has_assignments");
        }

        dbContext.Employees.Remove(employee);
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

    private static EmployeeResponse Map(Employee employee)
        => new()
        {
            EmployeeId = employee.EmployeeId,
            CompanyId = employee.CompanyId,
            Title = employee.Title,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Phone = employee.Phone,
            Email = employee.Email,
            IsActive = employee.IsActive
        };

    private static string NormalizeText(string value)
    {
        var result = value.Trim();

        if (string.IsNullOrWhiteSpace(result))
        {
            throw AppException.BadRequest("Text field cannot be empty.", "empty_text");
        }

        return result;
    }

    private static string NormalizeEmail(string value)
    {
        var result = value.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(result))
        {
            throw AppException.BadRequest("Email is required.", "employee_email_required");
        }

        return result;
    }

    private static string? NormalizeNullable(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
