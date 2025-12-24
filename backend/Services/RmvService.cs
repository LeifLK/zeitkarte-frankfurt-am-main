using RmvApiBackend.Models;
using RmvApiBackend.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Json; // For the .GetFromJsonAsync() helper
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace RmvApiBackend.Services
{
    /// <summary>
    /// This class implements the IRmvService. It does the actual work
    /// of calling the RMV API.
    /// </summary>
    public class RmvService : IRmvService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly RmvApiSettings _apiSettings;
        private readonly ILogger<RmvService> _logger;

        // We use "Dependency Injection" to get the tools we need.
        public RmvService(
            IHttpClientFactory httpClientFactory,
            IOptions<RmvApiSettings> apiSettings,
            ILogger<RmvService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            // Get the actual settings object
            _apiSettings = apiSettings.Value;
        }

        public async Task<RmvLocationResponse?> FindLocationAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(_apiSettings.ApiKey) || _apiSettings.ApiKey == "YOUR_API_KEY_GOES_HERE")
            {
                _logger.LogError("RMV API Key is not configured in appsettings.json");
                // In a real app, you might throw a custom exception
                return null;
            }

            // Use the "named" HttpClient we will register in Program.cs
            var client = _httpClientFactory.CreateClient("RMV");

            // Build the query string safely.
            // Uri.EscapeDataString handles spaces and special characters.
            string endpoint = $"location.name?input={Uri.EscapeDataString(searchTerm)}&accessId={_apiSettings.ApiKey}&format=json";

            try
            {
                // Make the API call and deserialize the JSON response directly.
                // This is a helper method from System.Net.Http.Json
                var response = await client.GetFromJsonAsync<RmvLocationResponse>(endpoint);
                return response;
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Error calling RMV API for term: {SearchTerm}", searchTerm);
                return null; // Return null or throw an exception
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An unexpected error occurred while finding location: {SearchTerm}", searchTerm);
                return null;
            }
        }
    }
}
