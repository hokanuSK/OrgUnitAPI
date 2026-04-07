namespace CompanyStructure.Api.Domain.Entities;

public sealed class Employee
{
    public int EmployeeId { get; set; }
    public int CompanyId { get; set; }

    public string? Title { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Company Company { get; set; } = default!;

    public ICollection<OrgUnit> ManagedOrgUnits { get; set; } = new List<OrgUnit>();
    public ICollection<EmployeeOrgUnitAssignment> OrgUnitAssignments { get; set; } = new List<EmployeeOrgUnitAssignment>();
}
