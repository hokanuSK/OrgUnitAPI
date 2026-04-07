using CompanyStructure.Api.Domain.Enums;

namespace CompanyStructure.Api.Contracts.OrgUnits;

public sealed class OrgUnitResponse
{
    public int OrgUnitId { get; set; }
    public int CompanyId { get; set; }
    public OrgUnitType UnitType { get; set; }
    public int? ParentOrgUnitId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? LeaderEmployeeId { get; set; }
}
