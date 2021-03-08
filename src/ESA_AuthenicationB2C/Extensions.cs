using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;

namespace ESA_AuthenicationB2C
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddGraphApiService(this IServiceCollection services, IConfiguration configuration)
        {
             // Initialize the client credential auth provider
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(configuration.GetValue<string>("MicrosoftGraph:AppId"))
                .WithTenantId(configuration.GetValue<string>("MicrosoftGraph:TenantId"))
                .WithClientSecret(configuration.GetValue<string>("MicrosoftGraph:ClientSecret"))
                .Build();
            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);

            // Set up the Microsoft Graph service client with client credentials
            services.Add(new ServiceDescriptor(typeof(IGraphServiceClient), sp => new GraphServiceClient(authProvider), ServiceLifetime.Transient));

            return services;
        }
    }
}