using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHotelSystem.Services;
using SmartHotelSystem.ViewModels;

namespace SmartHotelSystem.Controllers
{
    [Authorize]
    public class RecommendationController : Controller
    {
        private readonly RecommendationService _recommendationService;

        public RecommendationController(RecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        // GET
        public IActionResult Index()
        {
            return View(new RecommendationViewModel());
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RecommendationViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.Recommendation = await _recommendationService.RecommendAsync(
                model.Budget,
                model.Guests,
                model.Preference);

            return View(model);
        }
    }
}