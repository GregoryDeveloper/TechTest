using Microsoft.Extensions.DependencyInjection;
using Services.Applications.Utilities;

namespace Services.Applications.Validators
{
    public static class DependencyInjection
    {
        public static IServiceCollection Register(this IServiceCollection services)
        {
            // Services like IKycService and IBus would also need to be registered here
            return services
                .AddTransient<IDateTimeProvider, DateTimeProvider>()
                .AddTransient<IApplicationValidator, ApplicationValidator>();
        }
    }
}
