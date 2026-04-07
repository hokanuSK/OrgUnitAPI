using System.ComponentModel.DataAnnotations;
using CompanyStructure.Api.Domain.Enums;

namespace CompanyStructure.Api.Contracts.OrgUnits;

public sealed class CreateOrgUnitRequest
{
    [Required]
    public OrgUnitType UnitType { get; set; }

    public int? ParentOrgUnitId { get; set; }

    [Required]
    [MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public int? LeaderEmployeeId { get; set; }
}
