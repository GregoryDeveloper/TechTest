using Services.Common.Abstractions.Model;

namespace Services.Applications.Validators
{
    public interface IApplicationValidator
    {
        public Task<bool> Validate(User user, ProductCode product, Money money);
    }
}
