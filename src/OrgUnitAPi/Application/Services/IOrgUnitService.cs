using CompanyStructure.Api.Contracts.OrgUnits;

namespace CompanyStructure.Api.Application.Services;

public interface IOrgUnitService
{
    Task<OrgUnitResponse> CreateAsync(int companyId, CreateOrgUnitRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<OrgUnitResponse>> GetByCompanyAsync(int companyId, CancellationToken cancellationToken);
    Task<OrgUnitResponse> GetByIdAsync(int orgUnitId, CancellationToken cancellationToken);
    Task<OrgUnitResponse> UpdateAsync(int orgUnitId, UpdateOrgUnitRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(int orgUnitId, CancellationToken cancellationToken);
}
