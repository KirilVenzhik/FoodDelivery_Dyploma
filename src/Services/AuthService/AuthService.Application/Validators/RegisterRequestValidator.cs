using AuthService.Application.DTOs;
using AuthService.Domain.Enums;
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
            .Must(role => role is UserRole.Customer or UserRole.RestaurantOwner or UserRole.Courier)
            .WithMessage("Role must be Customer, RestaurantOwner or Courier.");
    }
}
