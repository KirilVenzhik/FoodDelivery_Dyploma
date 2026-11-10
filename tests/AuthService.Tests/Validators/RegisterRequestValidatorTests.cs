using AuthService.Application.DTOs;
using AuthService.Application.Validators;
using AuthService.Domain.Enums;
using FluentValidation.TestHelper;

namespace AuthService.Tests.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    private static RegisterRequest MakeRequest(UserRole role)
        => new() { Email = "user@test.com", Password = "secret123", Role = role };

    [Theory]
    [InlineData(UserRole.Customer)]
    [InlineData(UserRole.RestaurantOwner)]
    [InlineData(UserRole.Courier)]
    public void Validate_WithAllowedRole_HasNoRoleError(UserRole role)
    {
        var result = _validator.TestValidate(MakeRequest(role));

        result.ShouldNotHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public void Validate_WithAdminRole_HasRoleError()
    {
        var result = _validator.TestValidate(MakeRequest(UserRole.Admin));

        result.ShouldHaveValidationErrorFor(x => x.Role)
            .WithErrorMessage("Role must be Customer, RestaurantOwner or Courier.");
    }

    [Fact]
    public void Validate_WithUndefinedRole_HasRoleError()
    {
        var result = _validator.TestValidate(MakeRequest((UserRole)99));

        result.ShouldHaveValidationErrorFor(x => x.Role);
    }
}
