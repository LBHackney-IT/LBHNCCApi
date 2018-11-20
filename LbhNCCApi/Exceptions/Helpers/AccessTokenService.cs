using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LbhNCCApi.Helpers;
using System.Net.Http;
using System.Net.Http.Headers;
using LbhNCCApi.Interfaces;

namespace LbhNCCApi.Actions
{
    public class AccessTokenService : ICRMClientActions
    {
        public HttpClient GetCRMClient(bool formatter=false)
        {
            AccessTokenService access = new AccessTokenService();
            var request = access.BuildAccessTokenRequest();
            var accessToken = access.GetAccessToken(request).Result;
            return access.CreateRequest(accessToken, formatter).Result;
        }
        public async Task<string> GetAccessToken(CRM365AccessTokenRequest accessTokenRequest)
        {
            ClientCredential clientCredential = new ClientCredential(accessTokenRequest.ClientId, accessTokenRequest.ApplicationKey);
            AuthenticationContext authenticationContext = new AuthenticationContext(accessTokenRequest.ApplicationInstance + accessTokenRequest.TenantId);
            AuthenticationResult authenticationResult = authenticationContext.AcquireTokenAsync(accessTokenRequest.OrganizationUrl, clientCredential).Result;
            var requestedToken = authenticationResult.AccessToken;
            return requestedToken;
        }

        public CRM365AccessTokenRequest BuildAccessTokenRequest()
        {
            return new CRM365AccessTokenRequest
            {
                ApplicationInstance = Environment.GetEnvironmentVariable("WindowAzureappInstance"),
                ApplicationKey = Environment.GetEnvironmentVariable("WindowAzureappKey"),
                ClientId = Environment.GetEnvironmentVariable("WindowAzureClientId"),
                OrganizationUrl = Environment.GetEnvironmentVariable("CRM365OrganizationUrl"),
                TenantId = Environment.GetEnvironmentVariable("WindowAzuretenantID"),
            };
        }

        public async Task<HttpClient> CreateRequest(string accessToken, bool formatter)
        {
            HttpClient _client = new HttpClient();
            _client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("CRM365OrganizationUrl"));
            _client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            _client.DefaultRequestHeaders.Add("OData-Version", "4.0");
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            if(formatter)
            {
                _client.DefaultRequestHeaders.Add("Prefer", "return=representation");
                _client.DefaultRequestHeaders.Add("Prefer",
                        "odata.include-annotations=\"OData.Community.Display.V1.FormattedValue\"");
            }
            return _client;
        }

    }
}
