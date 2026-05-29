using FluentValidation;
using RestaurantService.Application.Commands;

namespace RestaurantService.Application.Validators;

public sealed class CreateRestaurantCommandValidator : AbstractValidator<CreateRestaurantCommand>
{
    public CreateRestaurantCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(200);
    }
}
