using CompanyStructure.Api.Contracts.OrgUnits;

namespace CompanyStructure.Api.Contracts.Companies;

public sealed class CompanyOrgTreeResponse
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public List<OrgUnitTreeNodeResponse> Divisions { get; set; } = [];
}
