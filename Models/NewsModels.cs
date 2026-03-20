namespace SmartInsuranceHub.Models
{
    public class NewsArticle
    {
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string? Content { get; set; }
        public string? Url { get; set; }
        public string? ImageUrl { get; set; }
        public string? Source { get; set; }
        public string? PublishedAt { get; set; }
    }

    public class NewsPageViewModel
    {
        public List<NewsArticle> Articles { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }
}
