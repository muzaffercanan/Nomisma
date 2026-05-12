using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nomisma.Application.Installments;
using Nomisma.Domain.Enums;

namespace Nomisma.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class InstallmentsController : ControllerBase
{
    private readonly IInstallmentService _installmentService;

    public InstallmentsController(IInstallmentService installmentService)
    {
        _installmentService = installmentService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InstallmentResponseDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _installmentService.GetAsync(id, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<InstallmentResponseDto>> Update(Guid id, UpdateInstallmentRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await _installmentService.UpdateAsync(id, request, cancellationToken));
    }
}
