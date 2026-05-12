using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nomisma.Application.Customers;
using Nomisma.Domain.Enums;

namespace Nomisma.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<IReadOnlyList<CustomerResponseDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _customerService.ListAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<CustomerResponseDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _customerService.GetAsync(id, cancellationToken));
    }

    [HttpGet("{id:guid}/summary")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<CustomerSummaryResponseDto>> GetSummary(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _customerService.GetSummaryAsync(id, cancellationToken));
    }

    [HttpGet("me/summary")]
    [Authorize(Roles = nameof(UserRole.Customer))]
    public async Task<ActionResult<CustomerSummaryResponseDto>> GetMySummary(CancellationToken cancellationToken)
    {
        return Ok(await _customerService.GetMySummaryAsync(cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<CustomerResponseDto>> Create(CreateCustomerRequestDto request, CancellationToken cancellationToken)
    {
        var customer = await _customerService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = customer.Id }, customer);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<CustomerResponseDto>> Update(Guid id, UpdateCustomerRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _customerService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _customerService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
