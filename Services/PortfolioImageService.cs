using MWBlazorPortfolioSite.Models;
using System.Net.Http.Json;

namespace MWBlazorPortfolioSite.Services
{
    public class PortfolioImageService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly Task<List<PortfolioImage>?> _getPortfolioImagesTask;

        public PortfolioImageService(HttpClient http)
        {
            _httpClient = http;

            _getPortfolioImagesTask = _httpClient.GetFromJsonAsync<List<PortfolioImage>>(
                "/Data/portfolioImageProperties.json");
        }

        internal async Task<PortfolioImage?> GetPortfolioImagesAsync(Func<PortfolioImage, bool> predicate)
        {
            var portfolioImages = await _getPortfolioImagesTask;
            return portfolioImages?.FirstOrDefault(predicate);
        }

        public void Dispose() => _httpClient.Dispose();

    }
}
