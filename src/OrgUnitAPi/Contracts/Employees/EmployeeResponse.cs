namespace CompanyStructure.Api.Contracts.Employees;

public sealed class EmployeeResponse
{
    public int EmployeeId { get; set; }
    public int CompanyId { get; set; }
    public string? Title { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
