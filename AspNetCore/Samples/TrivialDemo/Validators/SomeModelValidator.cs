using FluentValidation;
using TrivialDemo.Controllers;

namespace TrivialDemo.Validators
{
    public class SomeModelValidator : AbstractValidator<SomeModel>
    {
        public SomeModelValidator()
        {
            RuleFor(p => p.FirstName).NotEmpty();
            RuleFor(p => p.Email).EmailAddress();
        }
    }
}
