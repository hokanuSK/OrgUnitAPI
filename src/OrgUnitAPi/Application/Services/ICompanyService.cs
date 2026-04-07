using CompanyStructure.Api.Contracts.Companies;

namespace CompanyStructure.Api.Application.Services;

public interface ICompanyService
{
    Task<CompanyResponse> CreateAsync(CreateCompanyRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CompanyResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<CompanyResponse> GetByIdAsync(int companyId, CancellationToken cancellationToken);
    Task<CompanyResponse> UpdateAsync(int companyId, UpdateCompanyRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(int companyId, CancellationToken cancellationToken);
    Task<CompanyOrgTreeResponse> GetOrgTreeAsync(int companyId, CancellationToken cancellationToken);
}
