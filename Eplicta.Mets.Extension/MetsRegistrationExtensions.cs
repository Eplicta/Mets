using Microsoft.Extensions.DependencyInjection;

namespace Eplicta.Mets.Extensions;

public static class MetsRegistrationExtensions
{
    public static void AddEplictaMets(this IServiceCollection services)
    {
		services.AddTransient<IHttpClientFactory>(sp => new MicrosoftHttpClientFactory(sp.GetRequiredService<System.Net.Http.IHttpClientFactory>()));

		services.AddTransient<IMetsValidatorService, MetsValidatorService>();
    }
}