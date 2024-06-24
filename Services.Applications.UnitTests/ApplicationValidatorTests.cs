using Services.Applications.Utilities;
using Services.Applications.Validators;
using Services.Common.Abstractions.Model;
using NSubstitute;
using FluentAssertions;
using Services.Common.Abstractions.Abstractions;

namespace Services.Applications.UnitTests
{
    public class ApplicationValidatorTests
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IKycService _kycService;


        private ApplicationValidator _applicationValidator;

        public ApplicationValidatorTests()
        {
            _dateTimeProvider = Substitute.For<IDateTimeProvider>();
            _kycService = Substitute.For<IKycService>();
            _applicationValidator = new ApplicationValidator(_dateTimeProvider, _kycService);
        }

        public static IEnumerable<object[]> GeneratedProductOneData => new List<object[]>
        {
            new object[] {new DateOnly(2006,6,20), ProductCode.ProductOne, true },
            new object[] { new DateOnly(2006, 6, 21), ProductCode.ProductOne,true},
            new object[] { new DateOnly(2006, 6, 22), ProductCode.ProductTwo, false },
            new object[] {new DateOnly(1984,6,21), ProductCode.ProductOne, false },
            new object[] { new DateOnly(1985, 6, 21), ProductCode.ProductOne,true},
            new object[] { new DateOnly(1985, 6, 22), ProductCode.ProductOne, true },
        };

        [Theory]
        [MemberData(nameof(GeneratedProductOneData))]
        public async Task WhenValidate_WithProductOne_ShouldReturnExpected(DateOnly dateOfBirth, ProductCode product, bool expected)
        {
            // Arrange
            var money = new Money(string.Empty, 1m);
            var user = new User { DateOfBirth = dateOfBirth, IsVerified = true };
            var report = new KycReport (Guid.NewGuid(), true );
            var reportResult = Result.Success(report);

            _dateTimeProvider.Now.Returns(new DateTime(2024, 06, 21));
            _kycService.GetKycReportAsync(Arg.Any<User>()).Returns(reportResult);

            // Act
            var result = await _applicationValidator.Validate(user, product, money);

            // Assert
            result.Should().Be(expected);
        }

        public static IEnumerable<object[]> GeneratedProductTwoData => new List<object[]>
        {
            new object[] {new DateOnly(2006,6,20), ProductCode.ProductTwo, true },
            new object[] { new DateOnly(2006, 6, 21), ProductCode.ProductTwo, true},
            new object[] { new DateOnly(2006, 6, 22), ProductCode.ProductTwo, false },
            new object[] {new DateOnly(1973,6, 21), ProductCode.ProductTwo, false },
            new object[] { new DateOnly(1974, 6, 21), ProductCode.ProductTwo,true},
            new object[] { new DateOnly(1974, 6, 22), ProductCode.ProductTwo, true },
        };

        [Theory]
        [MemberData(nameof(GeneratedProductTwoData))]
        public async Task WhenValidate_WithProductTwo_ShouldReturnExpected(DateOnly dateOfBirth, ProductCode product, bool expected)
        {
            // Arrange
            var money = new Money(string.Empty, 1m);
            var user = new User { DateOfBirth = dateOfBirth, IsVerified = true };

            var report = new KycReport(Guid.NewGuid(), true);
            var reportResult = Result.Success(report);

            _dateTimeProvider.Now.Returns(new DateTime(2024, 06, 21));
            _kycService.GetKycReportAsync(Arg.Any<User>()).Returns(reportResult);

            // Act
            var result = await _applicationValidator.Validate(user, product, money);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(0.98,false)]
        [InlineData(1, true)]
        public async Task WhenValidate_WithPayment_ShouldReturnExpected(decimal amount, bool expected)
        {
            // Arrange
            var dateOfBirth = new DateOnly(2006, 6, 20);
            var user = new User { DateOfBirth = dateOfBirth, IsVerified = true };
            var report = new KycReport(Guid.NewGuid(), true);
            var reportResult = Result.Success(report);

            var money = new Money(string.Empty, amount);

            _dateTimeProvider.Now.Returns(new DateTime(2024, 06, 21));
            _kycService.GetKycReportAsync(Arg.Any<User>()).Returns(reportResult);

            // Act
            var result = await _applicationValidator.Validate(user, ProductCode.ProductOne, money);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public async Task WhenValidate_WithKycErrors_ShouldNotBeValid()
        {
            // Arrange
            var dateOfBirth = new DateOnly(2006, 6, 20);
            var user = new User { DateOfBirth = dateOfBirth };
            var report = new KycReport(Guid.NewGuid(), true);
            var error = new Error(string.Empty, string.Empty, string.Empty);
            Result <KycReport> reportResult = Result.Failure<KycReport>(error);

            var money = new Money(string.Empty, 1m);

            _dateTimeProvider.Now.Returns(new DateTime(2024, 06, 21));
            _kycService.GetKycReportAsync(Arg.Any<User>()).Returns(reportResult);

            // Act
            var result = await _applicationValidator.Validate(user, ProductCode.ProductOne, money);

            // Assert
            result.Should().Be(false);
        }

    }
}