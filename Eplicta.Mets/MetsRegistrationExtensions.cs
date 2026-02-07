using Microsoft.Extensions.DependencyInjection;

namespace Eplicta.Mets;

public static class MetsRegistrationExtensions
{
    public static void AddEplictaMets(this IServiceCollection services)
    {
        services.AddTransient<IMetsValidatorService, MetsValidatorService>();
    }
}