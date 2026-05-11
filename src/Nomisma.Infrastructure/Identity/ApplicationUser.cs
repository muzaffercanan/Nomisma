using Microsoft.AspNetCore.Identity;

namespace Nomisma.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public Guid? CustomerId { get; set; }
}

