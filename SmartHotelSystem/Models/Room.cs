using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SmartHotelSystem.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Room Number")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;   // Single, Double, Deluxe, Suite

        [Precision(18, 2)]
        [Display(Name = "Price Per Night")]
        public decimal PricePerNight { get; set; }

        [Display(Name = "Maximum Guests")]
        public int Capacity { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        [StringLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

    }
}