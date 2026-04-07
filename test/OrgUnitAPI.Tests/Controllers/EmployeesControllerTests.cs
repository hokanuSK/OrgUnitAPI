using CompanyStructure.Api.Application.Services;
using CompanyStructure.Api.Contracts.Employees;
using CompanyStructure.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace CompanyStructure.Api.Tests.Controllers;

public sealed class EmployeesControllerTests
{
    [Fact]
    public async Task Update_ReturnsOkWithEmployee()
    {
        var service = Substitute.For<IEmployeeService>();
        var employeeId = 21;
        var request = new UpdateEmployeeRequest
        {
            Title = "Developer",
            FirstName = "Jane",
            LastName = "Doe",
            Phone = "+421123456",
            Email = "jane.doe@acme.com",
            IsActive = true
        };
        var response = new EmployeeResponse
        {
            EmployeeId = employeeId,
            CompanyId = 3,
            Title = "Developer",
            FirstName = "Jane",
            LastName = "Doe",
            Phone = "+421123456",
            Email = "jane.doe@acme.com",
            IsActive = true
        };
        var cancellationToken = new CancellationTokenSource().Token;

        service.UpdateAsync(employeeId, request, cancellationToken).Returns(response);
        var controller = new EmployeesController(service);

        var actionResult = await controller.Update(employeeId, request, cancellationToken);

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var payload = Assert.IsType<EmployeeResponse>(ok.Value);
        Assert.Equal(response.EmployeeId, payload.EmployeeId);
        Assert.Equal(response.Email, payload.Email);

        await service.Received(1).UpdateAsync(employeeId, request, cancellationToken);
    }
}
