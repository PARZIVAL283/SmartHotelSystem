using System.Text;
using SmartHotelSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace SmartHotelSystem.Services
{
    public class RecommendationService
    {
        private readonly AppDbContext _context;
        private readonly GeminiService _gemini;

        public RecommendationService(
            AppDbContext context,
            GeminiService gemini)
        {
            _context = context;
            _gemini = gemini;
        }

        public async Task<string> RecommendAsync(
            decimal budget,
            int guests,
            string preference)
        {
            var rooms = await _context.Rooms
                .Where(r => r.IsAvailable)
                .ToListAsync();

            var sb = new StringBuilder();

            sb.AppendLine("Available hotel rooms:");

            foreach (var room in rooms)
            {
                sb.AppendLine($"""
Room Number: {room.RoomNumber}
Category: {room.Category}
Price Per Night: {room.PricePerNight} BDT
Description: {room.Description}

--------------------------
""");
            }

            sb.AppendLine($"""
Customer Requirements

Budget: {budget} BDT
Guests: {guests}
Preference: {preference}

Choose ONLY ONE room from the list above.

Explain why it is the best choice.

Do not invent rooms that are not listed.

keep the tone way easy and polite
""");

            return await _gemini.AskGeminiAsync(sb.ToString());
        }
    }
}