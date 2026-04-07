namespace CompanyStructure.Api.Domain.Entities;

public sealed class EmployeeOrgUnitAssignment
{
    public int EmployeeId { get; set; }
    public int OrgUnitId { get; set; }
    public int CompanyId { get; set; }

    public DateOnly AssignedFrom { get; set; }
    public DateOnly? AssignedTo { get; set; }
    public bool IsPrimary { get; set; }

    public Employee Employee { get; set; } = default!;
    public OrgUnit OrgUnit { get; set; } = default!;
}
