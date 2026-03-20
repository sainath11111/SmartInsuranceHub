using SmartInsuranceHub.Configuration;
using SmartInsuranceHub.Models;
using System.Text.Json;

namespace SmartInsuranceHub.ApiClients
{
    /// <summary>
    /// Client for the GNews API (https://gnews.io).
    /// Fetches insurance and finance related news articles.
    /// </summary>
    public class GNewsApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly NewsApiConfig _config;
        private readonly ILogger<GNewsApiClient> _logger;
        private const string BaseUrl = "https://gnews.io/api/v4";

        public GNewsApiClient(
            IHttpClientFactory httpClientFactory,
            NewsApiConfig config,
            ILogger<GNewsApiClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Fetches the latest insurance and finance news articles from GNews.
        /// </summary>
        public async Task<(List<NewsArticle> Articles, string? Error)> GetInsuranceNewsAsync(int maxArticles = 10)
        {
            if (!_config.HasGNewsKey)
            {
                return (new List<NewsArticle>(), "GNews API key is not configured. Please add GNEWS_API_KEY to the .env file.");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(15);

                var url = $"{BaseUrl}/search?q=insurance OR finance&lang=en&max={maxArticles}&sortby=publishedAt&apikey={_config.GNewsApiKey}";

                _logger.LogInformation("Fetching news from GNews API...");
                var response = await client.GetAsync(url);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("GNews API rate limit reached.");
                    return (new List<NewsArticle>(), "News API rate limit reached. Please try again later.");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning("GNews API key is invalid or expired.");
                    return (new List<NewsArticle>(), "Invalid API key. Please check your GNEWS_API_KEY in the .env file.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("GNews API returned status {Status}", response.StatusCode);
                    return (new List<NewsArticle>(), "Unable to load news. Please try again later.");
                }

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var articles = new List<NewsArticle>();

                if (root.TryGetProperty("articles", out var articlesArray))
                {
                    foreach (var a in articlesArray.EnumerateArray())
                    {
                        var title = a.TryGetProperty("title", out var t) ? t.GetString() : null;
                        var description = a.TryGetProperty("description", out var d) ? d.GetString() : null;
                        var content = a.TryGetProperty("content", out var c) ? c.GetString() : null;
                        var articleUrl = a.TryGetProperty("url", out var u) ? u.GetString() : "#";
                        var imageUrl = a.TryGetProperty("image", out var img) ? img.GetString() : null;
                        var publishedAt = a.TryGetProperty("publishedAt", out var pub) ? pub.GetString() : null;

                        // GNews source is an object: { "name": "...", "url": "..." }
                        string? source = "Unknown";
                        if (a.TryGetProperty("source", out var src) && src.TryGetProperty("name", out var sn))
                        {
                            source = sn.GetString();
                        }

                        if (!string.IsNullOrWhiteSpace(title))
                        {
                            articles.Add(new NewsArticle
                            {
                                Title = title!,
                                Description = description,
                                Content = content,
                                Url = articleUrl,
                                ImageUrl = imageUrl,
                                Source = source,
                                PublishedAt = publishedAt
                            });
                        }
                    }
                }

                _logger.LogInformation("Fetched {Count} articles from GNews.", articles.Count);
                return (articles, null);
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("GNews API request timed out.");
                return (new List<NewsArticle>(), "News request timed out. Please try again.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while fetching GNews.");
                return (new List<NewsArticle>(), "Network error. Please check your internet connection.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching GNews.");
                return (new List<NewsArticle>(), "Unable to load news. Please try again later.");
            }
        }
    }
}
