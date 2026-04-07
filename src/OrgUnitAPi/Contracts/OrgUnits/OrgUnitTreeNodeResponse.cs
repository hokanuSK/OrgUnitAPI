using CompanyStructure.Api.Domain.Enums;

namespace CompanyStructure.Api.Contracts.OrgUnits;

public sealed class OrgUnitTreeNodeResponse
{
    public int OrgUnitId { get; set; }
    public OrgUnitType UnitType { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? LeaderEmployeeId { get; set; }
    public string? LeaderFullName { get; set; }
    public List<OrgUnitTreeNodeResponse> Children { get; set; } = [];
}
