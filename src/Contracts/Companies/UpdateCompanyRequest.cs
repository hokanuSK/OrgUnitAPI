using System.ComponentModel.DataAnnotations;

namespace CompanyStructure.Api.Contracts.Companies;

public sealed class UpdateCompanyRequest
{
    [Required]
    [MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public int? DirectorEmployeeId { get; set; }
}
