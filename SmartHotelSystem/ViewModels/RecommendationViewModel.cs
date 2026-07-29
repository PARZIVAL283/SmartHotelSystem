using System.ComponentModel.DataAnnotations;

namespace SmartHotelSystem.ViewModels
{
    public class RecommendationViewModel
    {
        [Required]
        [Range(1, 100000)]
        public decimal Budget { get; set; }

        [Required]
        [Range(1, 20)]
        public int Guests { get; set; }

        [Required]
        [Display(Name = "Preferences")]
        public string Preference { get; set; } = "";

        public string? Recommendation { get; set; }
    }
}