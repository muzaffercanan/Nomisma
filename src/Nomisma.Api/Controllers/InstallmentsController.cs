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
    private readonly InstallmentService _installmentService;

    public InstallmentsController(InstallmentService installmentService)
    {
        _installmentService = installmentService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InstallmentDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _installmentService.GetAsync(id, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<InstallmentDto>> Update(Guid id, UpdateInstallmentRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _installmentService.UpdateAsync(id, request, cancellationToken));
    }
}

