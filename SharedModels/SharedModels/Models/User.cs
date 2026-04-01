namespace SharedModels;
using FluentValidation;

public class User
{
    public Guid UserId { get; set; }
    public string? Login { get; set; }
    public string? Password { get; set; }
    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public string? Token { get; set; }
}

public class UserChangePasswordDto
{
    public Guid UserId { get; set; }
    public string? OldPassword { get; set; }
    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }
}

public class UserLoginDto
{
    public string? Login { get; set; }
    public string? Password { get; set; }
}

public class UserChangePasswordDtoValidator : AbstractValidator<UserChangePasswordDto>
{
    public UserChangePasswordDtoValidator()
    {
        RuleFor(dto => dto.UserId)
            .NotEmpty().WithMessage("O campo [ID] é obrigatório.");

        RuleFor(dto => dto.OldPassword)
            .NotEmpty().WithMessage("O campo [Senha] é obrigatório.");

        RuleFor(dto => dto.NewPassword)
            .NotEmpty().WithMessage("O campo [Nova Senha] é obrigatório.")
            .MinimumLength(6).WithMessage("A senha deve ter 6 caracteres ou mais.");

        RuleFor(dto => dto.ConfirmPassword)
            .NotEmpty().WithMessage("O campo [Confirmação da Senha] é obrigatório")
            .Equal(dto => dto.NewPassword).WithMessage("A senha e a confirmação da senha devem ser iguais.");
    }
}

public class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
{
    public UserLoginDtoValidator()
    {
        RuleFor(dto => dto.Login)
            .NotEmpty().WithMessage("O campo [Login] é obrigatório.");

        RuleFor(dto => dto.Password)
            .NotEmpty().WithMessage("O campo [Senha] é obrigatório.");
    }
}