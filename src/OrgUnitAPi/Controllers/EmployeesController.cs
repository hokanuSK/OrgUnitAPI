using CompanyStructure.Api.Application.Services;
using CompanyStructure.Api.Contracts.Employees;
using Microsoft.AspNetCore.Mvc;

namespace CompanyStructure.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class EmployeesController(IEmployeeService employeeService) : ControllerBase
{
    [HttpPost("companies/{companyId:int}/employees")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<EmployeeResponse>> Create(int companyId, [FromBody] CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var response = await employeeService.CreateAsync(companyId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.EmployeeId }, response);
    }

    [HttpGet("companies/{companyId:int}/employees")]
    [ProducesResponseType(typeof(IReadOnlyCollection<EmployeeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<EmployeeResponse>>> GetByCompany(int companyId, CancellationToken cancellationToken)
    {
        var response = await employeeService.GetByCompanyAsync(companyId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("employees/{id:int}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EmployeeResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await employeeService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpPut("employees/{id:int}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EmployeeResponse>> Update(int id, [FromBody] UpdateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var response = await employeeService.UpdateAsync(id, request, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("employees/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await employeeService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
