using AuthService.Application.DTOs;
using FluentValidation;

namespace AuthService.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6);

        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("Role must be a valid value (1=Customer, 2=RestaurantOwner, 3=Courier, 4=Admin).");
    }
}
