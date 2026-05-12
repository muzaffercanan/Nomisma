using FluentValidation;
using AppValidationException = Nomisma.Application.Common.Exceptions.ValidationException;

namespace Nomisma.Application.Common.Validation;

public static class FluentValidationExtensions
{
    public static async Task ValidateAndThrowApplicationExceptionAsync<T>(
        this IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);
        if (result.IsValid)
        {
            return;
        }

        var message = string.Join(" ", result.Errors.Select(error => error.ErrorMessage).Distinct());
        throw new AppValidationException(message);
    }
}
