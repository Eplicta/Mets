using Microsoft.Extensions.DependencyInjection;

namespace Eplicta.Mets.Extensions;

public static class MetsRegistrationExtensions
{
    public static void AddEplictaMets(this IServiceCollection services)
    {
        services.AddTransient<IMetsValidatorService, MetsValidatorService>();
    }
}