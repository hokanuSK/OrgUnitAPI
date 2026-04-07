using CompanyStructure.Api.Application.Services;
using CompanyStructure.Api.Contracts.Companies;
using Microsoft.AspNetCore.Mvc;

namespace CompanyStructure.Api.Controllers;

[ApiController]
[Route("api/companies")]
public sealed class CompaniesController(CompanyService companyService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CompanyResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CompanyResponse>> Create([FromBody] CreateCompanyRequest request, CancellationToken cancellationToken)
    {
        var response = await companyService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.CompanyId }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<CompanyResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<CompanyResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var response = await companyService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CompanyResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CompanyResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await companyService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CompanyResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CompanyResponse>> Update(int id, [FromBody] UpdateCompanyRequest request, CancellationToken cancellationToken)
    {
        var response = await companyService.UpdateAsync(id, request, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await companyService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:int}/org-tree")]
    [ProducesResponseType(typeof(CompanyOrgTreeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CompanyOrgTreeResponse>> GetOrgTree(int id, CancellationToken cancellationToken)
    {
        var response = await companyService.GetOrgTreeAsync(id, cancellationToken);
        return Ok(response);
    }
}
