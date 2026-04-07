using CompanyStructure.Api.Contracts.Employees;

namespace CompanyStructure.Api.Application.Services;

public interface IEmployeeService
{
    Task<EmployeeResponse> CreateAsync(int companyId, CreateEmployeeRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<EmployeeResponse>> GetByCompanyAsync(int companyId, CancellationToken cancellationToken);
    Task<EmployeeResponse> GetByIdAsync(int employeeId, CancellationToken cancellationToken);
    Task<EmployeeResponse> UpdateAsync(int employeeId, UpdateEmployeeRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(int employeeId, CancellationToken cancellationToken);
}
