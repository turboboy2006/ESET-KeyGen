using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json; // Requires System.Net.Http.Json NuGet package
using System.Threading.Tasks;
using RankTracker.WebAPI.DTOs; // Adjust if DTOs are in a different namespace/assembly

namespace RankTracker.Desktop
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseApiUrl;

        public ApiClient(string baseApiUrl = "https://localhost:7001/api") // Adjust port if necessary
        {
            _httpClient = new HttpClient();
            _baseApiUrl = baseApiUrl;
        }

        // Website Methods
        public async Task<List<WebsiteDto>> GetWebsitesAsync() =>
            await _httpClient.GetFromJsonAsync<List<WebsiteDto>>($"{_baseApiUrl}/websites");

        public async Task<WebsiteDto> CreateWebsiteAsync(CreateWebsiteDto website)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseApiUrl}/websites", website);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<WebsiteDto>();
        }

        public async Task UpdateWebsiteAsync(int id, UpdateWebsiteDto website) // Assuming UpdateWebsiteDto exists
        {
            var response = await _httpClient.PutAsJsonAsync($"{_baseApiUrl}/websites/{id}", website);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteWebsiteAsync(int id) =>
            (await _httpClient.DeleteAsync($"{_baseApiUrl}/websites/{id}")).EnsureSuccessStatusCode();

        // Keyword Methods
        public async Task<List<KeywordDto>> GetKeywordsAsync() =>
            await _httpClient.GetFromJsonAsync<List<KeywordDto>>($"{_baseApiUrl}/keywords");

        public async Task<KeywordDto> CreateKeywordAsync(CreateKeywordDto keyword)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseApiUrl}/keywords", keyword);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<KeywordDto>();
        }

        public async Task DeleteKeywordAsync(int id) =>
            (await _httpClient.DeleteAsync($"{_baseApiUrl}/keywords/{id}")).EnsureSuccessStatusCode();

        // Website-Keyword Association Methods
        public async Task AssociateKeywordWithWebsiteAsync(int websiteId, int keywordId) =>
            (await _httpClient.PostAsync($"{_baseApiUrl}/websites/{websiteId}/keywords/{keywordId}", null)).EnsureSuccessStatusCode();

        public async Task DisassociateKeywordFromWebsiteAsync(int websiteId, int keywordId) =>
            (await _httpClient.DeleteAsync($"{_baseApiUrl}/websites/{websiteId}/keywords/{keywordId}")).EnsureSuccessStatusCode();

        public async Task<List<KeywordDto>> GetKeywordsForWebsiteAsync(int websiteId) =>
            await _httpClient.GetFromJsonAsync<List<KeywordDto>>($"{_baseApiUrl}/websites/{websiteId}/keywords");

        // Ranking Methods
        public async Task<RankingLogDto> CheckRankAsync(int websiteId, int keywordId)
        {
            var request = new ManualRankCheckRequestDto { WebsiteId = websiteId, KeywordId = keywordId };
            var response = await _httpClient.PostAsJsonAsync($"{_baseApiUrl}/rankings/check", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<RankingLogDto>();
            }
            // Basic error handling:
            var errorContent = await response.Content.ReadAsStringAsync();
            return new RankingLogDto { Rank = null, Notes = $"API Error: {response.StatusCode}. Details: {errorContent?.Substring(0, Math.Min(errorContent.Length, 200))}" };
        }

        public async Task<List<RankingLogDto>> GetRankingHistoryAsync(RankingHistoryQueryDto query)
        {
            var queryParams = new List<string>();
            if (query.WebsiteId.HasValue) queryParams.Add($"websiteId={query.WebsiteId.Value}");
            if (query.KeywordId.HasValue) queryParams.Add($"keywordId={query.KeywordId.Value}");
            if (query.DateFrom.HasValue) queryParams.Add($"dateFrom={query.DateFrom.Value:yyyy-MM-dd}");
            if (query.DateTo.HasValue) queryParams.Add($"dateTo={query.DateTo.Value:yyyy-MM-dd}");

            string queryString = queryParams.Any() ? $"?{string.Join("&", queryParams)}" : "";

            var requestUri = $"{_baseApiUrl}/rankings/history{queryString}";
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.GetAsync(requestUri);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<RankingLogDto>>();
            }
            else
            {
                // string errorContent = await response.Content.ReadAsStringAsync();
                // Consider logging the errorContent or throwing a custom exception
                return new List<RankingLogDto>(); // Return empty list on error
            }
        }
    }
}
