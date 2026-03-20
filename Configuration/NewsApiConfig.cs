namespace SmartInsuranceHub.Configuration
{
    /// <summary>
    /// Reads news API key from environment variables loaded by DotNetEnv.
    /// </summary>
    public class NewsApiConfig
    {
        public string? GNewsApiKey => Environment.GetEnvironmentVariable("GNEWS_API_KEY");

        public bool HasGNewsKey => !string.IsNullOrWhiteSpace(GNewsApiKey)
                                   && GNewsApiKey != "your_gnews_api_key_here";
    }
}
