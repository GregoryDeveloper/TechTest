﻿using AutoFixture;
using NSubstitute;
using Services.AdministratorOne.Abstractions;
using Services.AdministratorOne.Abstractions.Model;
using Services.Applications.Validators;
using Services.Common.Abstractions.Abstractions;
using Services.Common.Abstractions.Model;
using Xunit;

namespace Services.Applications.Tests;

public class ProductOneTests
{
    private readonly IAdministrationService _administrationService;
    private readonly AdministratorTwo.Abstractions.IAdministrationService _administrationServiceTwo;
    private readonly IApplicationValidator _applicationValidator;
    private readonly IBus _bus;

    private readonly ApplicationProcessor _applicationProcessor;

    public ProductOneTests()
    {
        _administrationService = Substitute.For<IAdministrationService>();
        _administrationServiceTwo = Substitute.For<AdministratorTwo.Abstractions.IAdministrationService>(); ;
        _applicationValidator = Substitute.For<IApplicationValidator>(); ;
        _bus = Substitute.For<IBus>();

        _applicationProcessor = new ApplicationProcessor(_administrationService, _administrationServiceTwo, _applicationValidator, _bus);
    }

    [Fact]
    public async Task ApplicationFailed_is_published_when_Applicant_is_too_young()
    {
        // Arrange
        Fixture fixture = new Fixture();

        var userId = Guid.NewGuid();
        fixture.Customize<DateOnly>(x => x.FromFactory(() => new DateOnly()));

        var application = fixture.Create<Application>();

        _applicationValidator.Validate(Arg.Any<User>(), Arg.Any<ProductCode>(), Arg.Any<Money>())
            .Returns(false);

        // Act
        await _applicationProcessor.Process(application);

        // Assert
        await _applicationValidator.Received(1).Validate(Arg.Any<User>(), Arg.Any<ProductCode>(), Arg.Any<Money>());
        _administrationService.Received(0).CreateInvestor(Arg.Any<CreateInvestorRequest>());
        await _bus.Received(1).PublishAsync(Arg.Any<RequestFailed>());
    }

    [Fact]
    public async Task WhenProcessProductOne_ThenPublishMessage()
    {
        // Arrange
        Fixture fixture = new Fixture();

        var userId = Guid.NewGuid();
        var user = fixture.Build<User>()
            .With(x => x.Id, userId)
            .With(x => x.DateOfBirth, new DateOnly())
            .Create();

        var application = fixture.Build<Application>()
            .With(x => x.ProductCode, ProductCode.ProductOne)
            .With(x => x.Applicant, user)
            .Create();

        var createInvestorResponse = fixture.Create<CreateInvestorResponse>();

        _applicationValidator.Validate(Arg.Any<User>(), Arg.Any<ProductCode>(), Arg.Any<Money>())
            .Returns(true);
        _administrationService.CreateInvestor(Arg.Any<CreateInvestorRequest>())
            .Returns(createInvestorResponse);

        // Act
        await _applicationProcessor.Process(application);

        // Assert
        await _applicationValidator.Received(1).Validate(Arg.Any<User>(), Arg.Any<ProductCode>(), Arg.Any<Money>());
        _administrationService.Received(1).CreateInvestor(Arg.Any<CreateInvestorRequest>());
        await _bus.Received().PublishAsync(Arg.Is<InvestorCreated>(x => x.UserId == userId && x.InvestorId == createInvestorResponse.InvestorId));
    }
}