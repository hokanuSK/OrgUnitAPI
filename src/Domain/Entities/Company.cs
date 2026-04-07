namespace CompanyStructure.Api.Domain.Entities;

public sealed class Company
{
    public int CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public int? DirectorEmployeeId { get; set; }
    public Employee? DirectorEmployee { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<OrgUnit> OrgUnits { get; set; } = new List<OrgUnit>();
}
