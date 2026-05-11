using Nomisma.Domain.Entities;

namespace Nomisma.Application.Abstractions.Integrations;

public interface ICreditScoreService
{
    Task<int> GetScoreAsync(Customer customer, CancellationToken cancellationToken);
}

