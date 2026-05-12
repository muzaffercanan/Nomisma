using Nomisma.Application.Installments;
using Nomisma.Domain.Entities;
using Nomisma.Domain.Enums;

namespace Nomisma.Application.Customers;

public static class CustomerMapper
{
    public static CustomerResponseDto ToDto(Customer customer) => new(
        customer.Id,
        customer.CustomerNumber,
        customer.FirstName,
        customer.LastName,
        customer.FullName,
        customer.NationalId,
        customer.Email,
        customer.PhoneNumber,
        customer.Address,
        customer.DateOfBirth,
        customer.CreatedAtUtc,
        customer.UpdatedAtUtc);

    public static CustomerSummaryResponseDto ToSummaryDto(Customer customer)
    {
        var installments = customer.Loans
            .SelectMany(loan => loan.Installments)
            .OrderBy(item => item.DueDate)
            .ThenBy(item => item.InstallmentNumber)
            .ToList();

        var paid = installments
            .Where(item => item.Status == InstallmentStatus.Paid)
            .Select(InstallmentMapper.ToDto)
            .ToList();

        var unpaid = installments
            .Where(item => item.Status != InstallmentStatus.Paid)
            .Select(InstallmentMapper.ToDto)
            .ToList();

        return new CustomerSummaryResponseDto(
            customer.Id,
            customer.CustomerNumber,
            customer.FullName,
            customer.Loans.Sum(loan => loan.TotalDebt),
            installments.Where(item => item.Status != InstallmentStatus.Paid).Sum(item => item.PrincipalAmount),
            installments.Where(item => item.Status != InstallmentStatus.Paid).Sum(item => item.Amount),
            installments.Count(item => item.Status == InstallmentStatus.Overdue),
            paid,
            unpaid);
    }
}
