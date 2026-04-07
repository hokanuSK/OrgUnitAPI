namespace CompanyStructure.Api.Contracts.Companies;

public sealed class CompanyResponse
{
    public int CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? DirectorEmployeeId { get; set; }
}
