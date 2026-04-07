using CompanyStructure.Api.Application.Services;
using CompanyStructure.Api.Contracts.OrgUnits;
using CompanyStructure.Api.Controllers;
using CompanyStructure.Api.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace CompanyStructure.Api.Tests.Controllers;

public sealed class OrgUnitsControllerTests
{
    [Fact]
    public async Task GetByCompany_ReturnsOkWithOrgUnits()
    {
        var service = Substitute.For<IOrgUnitService>();
        const int companyId = 10;
        IReadOnlyCollection<OrgUnitResponse> response =
        [
            new OrgUnitResponse
            {
                OrgUnitId = 100,
                CompanyId = companyId,
                UnitType = OrgUnitType.Division,
                Code = "DIV-01",
                Name = "Operations"
            }
        ];
        var cancellationToken = new CancellationTokenSource().Token;

        service.GetByCompanyAsync(companyId, cancellationToken).Returns(response);
        var controller = new OrgUnitsController(service);

        var actionResult = await controller.GetByCompany(companyId, cancellationToken);

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var payload = Assert.IsAssignableFrom<IReadOnlyCollection<OrgUnitResponse>>(ok.Value);
        Assert.Single(payload);
        Assert.Equal(100, payload.First().OrgUnitId);

        await service.Received(1).GetByCompanyAsync(companyId, cancellationToken);
    }
}
