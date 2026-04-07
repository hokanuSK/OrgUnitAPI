using CompanyStructure.Api.Application.Services;
using CompanyStructure.Api.Contracts.OrgUnits;
using Microsoft.AspNetCore.Mvc;

namespace CompanyStructure.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class OrgUnitsController(OrgUnitService orgUnitService) : ControllerBase
{
    [HttpPost("companies/{companyId:int}/org-units")]
    [ProducesResponseType(typeof(OrgUnitResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<OrgUnitResponse>> Create(int companyId, [FromBody] CreateOrgUnitRequest request, CancellationToken cancellationToken)
    {
        var response = await orgUnitService.CreateAsync(companyId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.OrgUnitId }, response);
    }

    [HttpGet("companies/{companyId:int}/org-units")]
    [ProducesResponseType(typeof(IReadOnlyCollection<OrgUnitResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<OrgUnitResponse>>> GetByCompany(int companyId, CancellationToken cancellationToken)
    {
        var response = await orgUnitService.GetByCompanyAsync(companyId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("org-units/{id:int}")]
    [ProducesResponseType(typeof(OrgUnitResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrgUnitResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await orgUnitService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpPut("org-units/{id:int}")]
    [ProducesResponseType(typeof(OrgUnitResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrgUnitResponse>> Update(int id, [FromBody] UpdateOrgUnitRequest request, CancellationToken cancellationToken)
    {
        var response = await orgUnitService.UpdateAsync(id, request, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("org-units/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await orgUnitService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
