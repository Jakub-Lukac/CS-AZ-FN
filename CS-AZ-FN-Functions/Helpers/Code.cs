using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_AZ_FN_Functions.Helpers
{
    public class Code
    {
        public GraphServiceClient GetAuthenticatedGraphClient(string tenantId, string clientId, string clientSecret)
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            };

            var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret, options);

            return new(clientSecretCredential, scopes);
        }

        public void LogHelper(string logMessage, List<string> listOfLogs, ILogger log)
        {
            listOfLogs.Add(logMessage);
            if (listOfLogs != null) 
                log.LogInformation(logMessage);
        }

        public Task<User> GetUser(GraphServiceClient graphServiceClient, string userId)
        {
            try
            {
                var result = graphServiceClient.Users[userId].GetAsync();
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<List<User>> GetUsers(GraphServiceClient graphClient, string groupId) 
        {
            List<string> logs = new List<string>();
            List<User> users = new List<User>();
            try
            {
                var usersResult = await graphClient.Groups[groupId].Members.GetAsync((requestConfiguration) =>
                {
                    requestConfiguration.QueryParameters.Top = 999;
                });

                users = users.Union(usersResult.Value.Where(w => w.GetType() == typeof(User) && (w as User).Mail != null).OrderBy(o => o.Id).Select(s => s as User).ToList()).ToList();

                var nextPageLink = usersResult.OdataNextLink;
                while (nextPageLink != null)
                {
                    var nextPageRequestInformation = new RequestInformation
                    {
                        HttpMethod = Method.GET,
                        UrlTemplate = nextPageLink
                    };

                    var nextPageResult = await graphClient.RequestAdapter.SendAsync(nextPageRequestInformation, (parseNode) => new UserCollectionResponse());
                    users = users.Union(nextPageResult.Value.Where(w => w.GetType() == typeof(User) && (w as User).Mail != null).OrderBy(o => o.Id).Select(s => s as User).ToList()).ToList();
                    nextPageLink = nextPageResult.OdataNextLink;
                }

                return users;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new List<User>();
            }
        }
    }
}
