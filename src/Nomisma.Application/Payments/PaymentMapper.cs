using Nomisma.Domain.Entities;

namespace Nomisma.Application.Payments;

public static class PaymentMapper
{
    public static PaymentResponseDto ToDto(Payment payment)
    {
        return ToDto(
            payment,
            payment.Installment ?? throw new InvalidOperationException("Payment installment is not loaded."),
            payment.Installment.Loan ?? throw new InvalidOperationException("Payment loan is not loaded."));
    }

    public static PaymentResponseDto ToDto(Payment payment, Installment installment, Loan loan)
    {
        return new PaymentResponseDto(
            payment.Id,
            payment.InstallmentId,
            installment.LoanId,
            loan.CustomerId,
            payment.Amount,
            payment.PaidAtUtc,
            payment.Status,
            payment.GatewayStatus,
            payment.GatewayTransactionId,
            payment.FailureReason);
    }
}
