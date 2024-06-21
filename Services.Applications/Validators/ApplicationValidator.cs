using Services.Applications.Utilities;
using Services.Common.Abstractions.Abstractions;
using Services.Common.Abstractions.Model;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Services.Applications.UnitTests")]
namespace Services.Applications.Validators
{
    internal class ApplicationValidator : IApplicationValidator
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IKycService _kycService;

        public ApplicationValidator(IDateTimeProvider dateTimeProvider, IKycService kycService)
        {
            _dateTimeProvider = dateTimeProvider;
            _kycService = kycService;
        }
        public async Task<bool> Validate(User user, ProductCode product, Money money)
        {
            if (!ValidateProduct(user.DateOfBirth, product))
            {
                return false;
            }
            if (!ValidatePayment(money))
            {
                return false;
            }
            if(!await IsUserKYCed(user))
            {
                return false;
            }
            if (user?.IsVerified == null || user.IsVerified == false)
            {
                return false;
            }

            return true;
        }

        private bool ValidateProduct(DateOnly dateOfBirth, ProductCode product)
        {
            // Age could have been a get property added to the User but it would mean the validation object would need to have the user object as a parameter
            // increasing the coupling between the validation and the object itself. Both options are valid in my opinion.
            int age = CalculateAge(dateOfBirth, DateOnly.FromDateTime(_dateTimeProvider.Now));
            switch (product)
            {
                case ProductCode.ProductOne:
                    if (age < 18 || age > 39)
                    {
                        return false;
                    }
                    break;
                case ProductCode.ProductTwo:
                    if (age < 18 || age > 50)
                    {
                        return false;
                    }
                    break;
                default:
                    // Could add logging error in this case.
                    return false;
            }

            return true;
        }

        private int CalculateAge(DateOnly birthDate, DateOnly currentDate)
        {
            int age = currentDate.Year - birthDate.Year;

            if (currentDate < birthDate.AddYears(age))
            {
                age--;
            }

            return age;
        }

        private bool ValidatePayment(Money money)
        {
            if (money.Amount >= 0.99m)
                return true;

            return false;
        }

        private async Task<bool> IsUserKYCed(User user)
        {
            Result<KycReport> reportResult = await _kycService.GetKycReportAsync(user);

            if (reportResult.IsSuccess)
            {
                var report = reportResult.Value;
                if (report != null && report.IsVerified)
                    return true;
            }
            else // we would log an error
            {
                return false;
            }
            
            return false;
        }
    }
}
