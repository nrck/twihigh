using FluentValidation;

namespace PheasantTails.TwiHigh.Data.Model.TwiHighUsers
{
    public class AddTwiHighUserContext
    {
        public string DisplayId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class AddTwiHighUserContextValidator : AbstractValidator<AddTwiHighUserContext>
    {
        public AddTwiHighUserContextValidator()
        {
            RuleFor(c => c.DisplayId)
                .NotEmpty()
                .OverridePropertyName("ID");
            RuleFor(c => c.DisplayName)
                .NotEmpty()
                .MaximumLength(50)
                .OverridePropertyName("表示名");
            RuleFor(c => c.Password)
                .NotEmpty()
                .Matches("^([a-zA-Z0-9!-/:-@¥[-`{-~]+)?$")
                .MinimumLength(8)
                .OverridePropertyName("パスワード");
            RuleFor(c => c.Email)
                .NotEmpty()
                .EmailAddress()
                .OverridePropertyName("E-mail");
        }
    }
}
