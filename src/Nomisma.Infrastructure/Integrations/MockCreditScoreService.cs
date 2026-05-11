using Nomisma.Application.Abstractions.Integrations;
using Nomisma.Domain.Entities;

namespace Nomisma.Infrastructure.Integrations;

public sealed class MockCreditScoreService : ICreditScoreService
{
    public Task<int> GetScoreAsync(Customer customer, CancellationToken cancellationToken)
    {
        if (customer.Email.Contains("risk", StringComparison.OrdinalIgnoreCase)
            || customer.NationalId.EndsWith('0'))
        {
            return Task.FromResult(580);
        }

        var checksum = customer.NationalId
            .Where(char.IsDigit)
            .Select(character => character - '0')
            .Sum();

        return Task.FromResult(700 + checksum % 101);
    }
}

