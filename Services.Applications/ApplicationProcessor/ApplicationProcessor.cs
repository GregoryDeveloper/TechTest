using Services.AdministratorOne.Abstractions;
using Services.AdministratorOne.Abstractions.Model;
using Services.Common.Abstractions.Abstractions;
using Services.Common.Abstractions.Model;

namespace Services.Applications.Validators
{
    internal class ApplicationProcessor : IApplicationProcessor
    {
        private readonly IAdministrationService _administrationService;
        private readonly AdministratorTwo.Abstractions.IAdministrationService _administrationServiceTwo;
        private readonly IApplicationValidator _applicationValidator;
        private readonly IBus _bus;

        public ApplicationProcessor(IAdministrationService administrationService, 
            AdministratorTwo.Abstractions.IAdministrationService administrationServiceTwo,
            IApplicationValidator applicationValidator,
            IBus bus)
        {
            _administrationService = administrationService;
            _administrationServiceTwo = administrationServiceTwo;
            _applicationValidator = applicationValidator;
            _bus = bus;
        }

        public async Task Process(Application application)
        {
            var isValid = await _applicationValidator.Validate(application.Applicant, application.ProductCode, application.Payment.Amount);
            DomainEvent? domainEvent = null;

            if (isValid)
            {
                if (application.ProductCode == ProductCode.ProductOne)
                {
                    var request = Map(application);
                    var response = _administrationService.CreateInvestor(request);
                    domainEvent = new InvestorCreated(application.Applicant.Id, response.InvestorId);
                }
                else if (application.ProductCode == ProductCode.ProductTwo)
                {
                    var request = Map(application);
                    // Assumed it returns the investorId
                    var requestResult = await _administrationServiceTwo.CreateInvestorAsync(application.Applicant);
                    if (requestResult.IsSuccess)
                    {
                        domainEvent = new InvestorCreated(application.Applicant.Id, requestResult.Value.ToString());
                    }
                }
            }

            if (domainEvent is null)
            {
                domainEvent = new RequestFailed(application.Id);
            }

            await _bus.PublishAsync(domainEvent);
        }

        private CreateInvestorRequest Map(Application application)
        {
            return new CreateInvestorRequest
            {
                FirstName = application.Applicant.Forename,
                LastName = application.Applicant.Surname,
                DateOfBirth = application.Applicant.DateOfBirth.ToString(),
                Nino = application.Applicant.Nino,
                Addressline1 = application.Applicant.Addresses.FirstOrDefault()?.Addressline1 ?? string.Empty,
                Addressline2 = application.Applicant.Addresses.FirstOrDefault()?.Addressline2 ?? string.Empty,
                Addressline3 = application.Applicant.Addresses.FirstOrDefault()?.Addressline3 ?? string.Empty,
                PostCode = application.Applicant.Addresses.FirstOrDefault()?.PostCode ?? string.Empty,
                Product = "ProductOne",
                SortCode = application.Payment.BankAccount.SortCode ?? string.Empty,
                AccountNumber = application.Payment.BankAccount.AccountNumber ?? string.Empty,
                InitialPayment = Convert.ToInt32(application.Payment.Amount.Amount)
            };
        }
    }
}
