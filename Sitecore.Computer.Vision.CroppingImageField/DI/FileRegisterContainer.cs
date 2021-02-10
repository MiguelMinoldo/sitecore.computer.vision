using Microsoft.Extensions.DependencyInjection;
using Sitecore.Computer.Vision.CroppingImageField.Services;
using Sitecore.DependencyInjection;

namespace Sitecore.Computer.Vision.CroppingImageField.DI
{
    public class RegisterContainer : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ICognitiveServices, CognitiveServices>();
            serviceCollection.AddTransient<ICroppingService, CroppingService>();
        }
    }
}