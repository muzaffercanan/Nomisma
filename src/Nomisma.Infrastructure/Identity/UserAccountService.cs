using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nomisma.Application.Abstractions.Auth;
using Nomisma.Application.Common.Exceptions;
using Nomisma.Domain.Enums;

namespace Nomisma.Infrastructure.Identity;

public sealed class UserAccountService : IUserAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserAccountService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task CreateCustomerUserAsync(string email, string password, Guid customerId, CancellationToken cancellationToken)
    {
        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            throw new ConflictException("Bu email icin kullanici zaten mevcut.");
        }

        var user = new ApplicationUser
        {
            Email = email,
            UserName = email,
            EmailConfirmed = true,
            CustomerId = customerId
        };

        var createResult = await _userManager.CreateAsync(user, password);
        EnsureSuccess(createResult, "Musteri kullanicisi olusturulamadi.");

        var roleResult = await _userManager.AddToRoleAsync(user, UserRole.Customer.ToString());
        EnsureSuccess(roleResult, "Musteri rolu atanamadi.");
    }

    public async Task UpdateCustomerEmailAsync(Guid customerId, string email, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(item => item.CustomerId == customerId, cancellationToken);
        if (user is null)
        {
            return;
        }

        user.Email = email;
        user.UserName = email;
        user.EmailConfirmed = true;
        var result = await _userManager.UpdateAsync(user);
        EnsureSuccess(result, "Musteri kullanici emaili guncellenemedi.");
    }

    public async Task DisableCustomerUserAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(item => item.CustomerId == customerId, cancellationToken);
        if (user is null)
        {
            return;
        }

        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;
        var result = await _userManager.UpdateAsync(user);
        EnsureSuccess(result, "Musteri kullanicisi pasife alinamadi.");
    }

    private static void EnsureSuccess(IdentityResult result, string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error => error.Description));
        throw new ConflictException($"{message} {errors}");
    }
}

