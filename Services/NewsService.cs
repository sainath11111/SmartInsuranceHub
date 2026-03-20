using Microsoft.Extensions.Caching.Memory;
using SmartInsuranceHub.ApiClients;
using SmartInsuranceHub.Models;

namespace SmartInsuranceHub.Services
{
    /// <summary>
    /// Fetches news from GNews API with in-memory caching (5-minute TTL)
    /// to respect API rate limits.
    /// </summary>
    public class NewsService
    {
        private readonly GNewsApiClient _gNewsClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<NewsService> _logger;

        private const string NewsCacheKey = "news_articles";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        public NewsService(
            GNewsApiClient gNewsClient,
            IMemoryCache cache,
            ILogger<NewsService> logger)
        {
            _gNewsClient = gNewsClient;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Gets news page data. Results are cached for 5 minutes to avoid hitting rate limits.
        /// </summary>
        public async Task<NewsPageViewModel> GetNewsPageDataAsync()
        {
            var cached = _cache.Get<(List<NewsArticle>, string?)>(NewsCacheKey);
            if (cached.Item1 != null)
            {
                _logger.LogInformation("Serving {Count} cached news articles.", cached.Item1.Count);
                return new NewsPageViewModel
                {
                    Articles = cached.Item1,
                    ErrorMessage = cached.Item2
                };
            }

            var (articles, error) = await _gNewsClient.GetInsuranceNewsAsync(10);
            _cache.Set(NewsCacheKey, (articles, error), CacheDuration);

            return new NewsPageViewModel
            {
                Articles = articles,
                ErrorMessage = error
            };
        }
    }
}
