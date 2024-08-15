using FluentValidation;
using leaderboard.Models;

namespace leaderboard.Validators
{
    public class CustomerValidator : AbstractValidator<UpdateCustomerScoreModel>
    {
        public CustomerValidator()
        {
            RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("customer id should greater than zero");
            RuleFor(x => x.Score).ExclusiveBetween(-1000, 1000).WithMessage("customer score should between -1000 and 1000");
        }
    }
}
