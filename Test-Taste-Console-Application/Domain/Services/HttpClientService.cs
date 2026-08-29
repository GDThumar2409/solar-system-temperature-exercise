using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Utilities;

namespace Test_Taste_Console_Application.Domain.Services
{
    ///<summary>
    /// A service to create the HttpClient. 
    ///</summary>
    public class HttpClientService
    {
        public HttpClient Client { get; }

        public HttpClientService(HttpClient client, IConfiguration configuration)
        {
            //The HTTP client is configured in the constructor.
            Client = client;
            Client.BaseAddress = new Uri(UriPath.BaseUri);
            Client.DefaultRequestHeaders.Accept.Add(new
                MediaTypeWithQualityHeaderValue(HttpClientSettings.JsonType));

            //The API now needs a free API key as a bearer token. It comes from
            //appsettings.json (git-ignored) or the env var, so it stays out of
            //source control. Without it every call is a 401.
            var apiKey = configuration[HttpClientSettings.ApiKeyConfigurationKey];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = Environment.GetEnvironmentVariable(HttpClientSettings.ApiKeyEnvironmentVariable);
            }

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());
            }
            else
            {
                Logger.Instance.Warn("No API key found in appsettings.json or the " +
                    $"{HttpClientSettings.ApiKeyEnvironmentVariable} env var; API calls will return 401.");
            }
        }
    }
}
