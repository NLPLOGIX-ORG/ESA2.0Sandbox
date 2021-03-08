using ESA_AuthenicationB2C.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ESA_AuthenicationB2C.Services
{
    public interface IGraphApiClient
    {
        Task<System.Collections.Generic.IList<User>> GetUser(string userId);
        Task<System.Collections.Generic.IList<User>> GetUsers();

        Task UpdateSecurityQuestions(string userObjectId);
    }

    public class GraphApiClient : IGraphApiClient
    {
        private IGraphServiceClient _graphServiceClient;
        private GraphApiOptions _graphApiOptions;

        internal GraphApiClient(IConfiguration config)
        {
            _graphApiOptions = new GraphApiOptions();
            config.GetSection(GraphApiOptions.MicrosoftGraph).Bind(_graphApiOptions);

             // Initialize the client credential auth provider
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(_graphApiOptions.AppId)
                .WithTenantId(_graphApiOptions.TenantId)
                .WithClientSecret(_graphApiOptions.ClientSecret)
                .Build();
            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);

            // Set up the Microsoft Graph service client with client credentials
            _graphServiceClient = new GraphServiceClient(authProvider);
        }

        public async Task<System.Collections.Generic.IList<User>> GetUser(string userId)
        {
            var result = await _graphServiceClient.Users
                .Request()
                .Filter($"identities/any(c:c/issuerAssignedId eq '{userId}' and c/issuer eq '{_graphApiOptions.B2cExtensionAppClientId}')")
                .Select($@"id,displayName,identities,
                    {GetCompleteAttributeName("securityQuestionOne")},
                    {GetCompleteAttributeName("securityQuestionOneAnswer")},
                    {GetCompleteAttributeName("securityQuestionTwo")},
                    {GetCompleteAttributeName("securityQuestionTwoAnswer")},
                    {GetCompleteAttributeName("securityQuestionThree")},
                    {GetCompleteAttributeName("securityQuestionThreeAnswer")}")
                .GetAsync();
            
            return result.CurrentPage;
        }

        public async Task<System.Collections.Generic.IList<User>> GetUsers()
        {
            var result = await _graphServiceClient.Users
                .Request()
                .Select($@"id,displayName,identities,
                    {GetCompleteAttributeName("securityQuestionOne")},
                    {GetCompleteAttributeName("securityQuestionOneAnswer")},
                    {GetCompleteAttributeName("securityQuestionTwo")},
                    {GetCompleteAttributeName("securityQuestionTwoAnswer")},
                    {GetCompleteAttributeName("securityQuestionThree")},
                    {GetCompleteAttributeName("securityQuestionThreeAnswer")}")
                .GetAsync();
            
            return result.CurrentPage;
        }

        public async Task UpdateSecurityQuestions(string userObjectId)
        {
            if (string.IsNullOrWhiteSpace(userObjectId))
            {
                throw new System.ArgumentException("Parameter cannot be null", nameof(userObjectId));
            }

            IDictionary<string, object> extensionInstance = new Dictionary<string, object>();
            extensionInstance.Add(GetCompleteAttributeName("securityQuestionOne"), "Is this a test?");
            extensionInstance.Add(GetCompleteAttributeName("securityQuestionOneAnswer"), "Why, yes it is!");

            var user = new User
            {
                AdditionalData = extensionInstance
            };

            try
            {
                // Update user by object ID
                await _graphServiceClient.Users[userObjectId]
                   .Request()
                   .UpdateAsync(user);
            }
            catch (System.Exception)
            {
               throw;
            }
        }

        #region Helpers

        internal string GetCompleteAttributeName(string attributeName)
        {
            if (string.IsNullOrWhiteSpace(attributeName))
            {
                throw new System.ArgumentException("Parameter cannot be null", nameof(attributeName));
            }

            return $"extension_{_graphApiOptions.B2cExtensionAppClientId.Replace("-", "")}_{attributeName}";
        }

        #endregion
    }
}