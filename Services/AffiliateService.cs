using System.Net.Http.Json;
using Elwala.Models;

namespace Elwala.Services
{
    public class AffiliateService : IAffiliateService
    {
        private readonly HttpClient _httpClient;
        public AffiliateService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AffiliateResponse> GenerateAffiliateLinkAsync(AffiliateRequest request)
        {
            try
            {
                // Using an absolute URL to avoid BaseAddress configuration issues
                var apiUrl = "https://api.ellwaa.com/api/assis/requests";
                
                var response = await _httpClient.PostAsJsonAsync(apiUrl, request);

                if (response.IsSuccessStatusCode)
                {
                    // Assuming the API returns a JSON matching AffiliateResponse
                    // or just a string URL. Here we assume it returns { affiliateUrl: "..." }
                    var result = await response.Content.ReadFromJsonAsync<AffiliateResponse>();
                    return result ?? new AffiliateResponse { Success = false, ErrorMessage = "Empty response from API." };
                }

                return new AffiliateResponse 
                { 
                    Success = false, 
                    ErrorMessage = $"API Error: {response.StatusCode}" 
                };
            }
            catch (Exception ex)
            {
                return new AffiliateResponse 
                { 
                    Success = false, 
                    ErrorMessage = $"Exception: {ex.Message}" 
                };
            }
        }
    }
}
