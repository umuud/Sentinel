using FluentValidation;

namespace Sentinel.Application.Features.Auth.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username boş olamaz")
            .MinimumLength(3);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email boş olamaz")
            .EmailAddress().WithMessage("Geçerli email gir");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password boş olamaz")
            .MinimumLength(6);
    }
}