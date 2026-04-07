using CompanyStructure.Api.Domain.Enums;

namespace CompanyStructure.Api.Domain.Entities;

public sealed class OrgUnit
{
    public int OrgUnitId { get; set; }
    public int CompanyId { get; set; }

    public int? ParentOrgUnitId { get; set; }
    public OrgUnit? ParentOrgUnit { get; set; }

    public OrgUnitType UnitType { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public int? LeaderEmployeeId { get; set; }
    public Employee? LeaderEmployee { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Company Company { get; set; } = default!;
    public ICollection<OrgUnit> Children { get; set; } = new List<OrgUnit>();
    public ICollection<EmployeeOrgUnitAssignment> EmployeeAssignments { get; set; } = new List<EmployeeOrgUnitAssignment>();
}
