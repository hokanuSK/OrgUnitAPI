using CompanyStructure.Api.Application.Services;
using CompanyStructure.Api.Contracts.Companies;
using CompanyStructure.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace CompanyStructure.Api.Tests.Controllers;

public sealed class CompaniesControllerTests
{
    [Fact]
    public async Task Create_ReturnsCreatedAtActionWithCompany()
    {
        var service = Substitute.For<ICompanyService>();
        var request = new CreateCompanyRequest
        {
            Code = "acme",
            Name = "Acme"
        };
        var response = new CompanyResponse
        {
            CompanyId = 12,
            Code = "ACME",
            Name = "Acme"
        };
        var cancellationToken = new CancellationTokenSource().Token;

        service.CreateAsync(request, cancellationToken).Returns(response);
        var controller = new CompaniesController(service);

        var actionResult = await controller.Create(request, cancellationToken);

        var created = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        Assert.Equal(nameof(CompaniesController.GetById), created.ActionName);
        Assert.NotNull(created.RouteValues);
        Assert.Equal(response.CompanyId, created.RouteValues["id"]);

        var payload = Assert.IsType<CompanyResponse>(created.Value);
        Assert.Equal(response.CompanyId, payload.CompanyId);

        await service.Received(1).CreateAsync(request, cancellationToken);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var service = Substitute.For<ICompanyService>();
        var cancellationToken = new CancellationTokenSource().Token;
        var controller = new CompaniesController(service);
        service.DeleteAsync(5, cancellationToken).Returns(Task.CompletedTask);

        var result = await controller.Delete(5, cancellationToken);

        Assert.IsType<NoContentResult>(result);
        await service.Received(1).DeleteAsync(5, cancellationToken);
    }
}
