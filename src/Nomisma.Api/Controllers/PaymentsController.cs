using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nomisma.Application.Payments;

namespace Nomisma.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PaymentResponseDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _paymentService.ListAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentResponseDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _paymentService.GetAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<PaymentResponseDto>> Create(CreatePaymentRequestDto request, CancellationToken cancellationToken)
    {
        var payment = await _paymentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = payment.Id }, payment);
    }
}
