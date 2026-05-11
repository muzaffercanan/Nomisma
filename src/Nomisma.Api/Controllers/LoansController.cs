using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nomisma.Application.Installments;
using Nomisma.Application.Loans;
using Nomisma.Domain.Enums;

namespace Nomisma.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class LoansController : ControllerBase
{
    private readonly LoanService _loanService;

    public LoansController(LoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LoanDto>>> List([FromQuery] Guid? customerId, CancellationToken cancellationToken)
    {
        return Ok(await _loanService.ListAsync(customerId, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LoanDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _loanService.GetAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<LoanDto>> Create(CreateLoanRequest request, CancellationToken cancellationToken)
    {
        var loan = await _loanService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = loan.Id }, loan);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<LoanDto>> Update(Guid id, UpdateLoanRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _loanService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpGet("{id:guid}/installments")]
    public async Task<ActionResult<IReadOnlyList<InstallmentDto>>> GetInstallments(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _loanService.GetInstallmentsAsync(id, cancellationToken));
    }
}

