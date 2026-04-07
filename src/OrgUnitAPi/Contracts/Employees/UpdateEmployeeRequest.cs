using System.ComponentModel.DataAnnotations;

namespace CompanyStructure.Api.Contracts.Employees;

public sealed class UpdateEmployeeRequest
{
    [MaxLength(50)]
    public string? Title { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Phone { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
