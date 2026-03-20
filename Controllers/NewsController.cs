using Microsoft.AspNetCore.Mvc;
using SmartInsuranceHub.Models;
using SmartInsuranceHub.Services;

namespace SmartInsuranceHub.Controllers
{
    public class NewsController : Controller
    {
        private readonly NewsService _newsService;

        public NewsController(NewsService newsService)
        {
            _newsService = newsService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = await _newsService.GetNewsPageDataAsync();
            return View(viewModel);
        }

        public IActionResult Detail(string title, string? image, string? description, string? url, string? source, string? date, string? content)
        {
            var article = new NewsArticle
            {
                Title = title ?? "Untitled",
                ImageUrl = image,
                Description = description,
                Content = content,
                Url = url,
                Source = source,
                PublishedAt = date
            };

            return View(article);
        }
    }
}
