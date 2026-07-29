using Microsoft.AspNetCore.Mvc;
using SmartHotelSystem.Services;

namespace SmartHotelSystem.Controllers
{
    public class AITestController : Controller
    {
        private readonly GeminiService _gemini;

        public AITestController(GeminiService gemini)
        {
            _gemini = gemini;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _gemini.AskGeminiAsync(
                "Say 'Hello from Gemini!' in one sentence.");

            return Content(result);
        }
    }
}