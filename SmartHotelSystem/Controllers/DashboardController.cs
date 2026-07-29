using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHotelSystem.Data;
using SmartHotelSystem.Models;

namespace SmartHotelSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalRooms = await _context.Rooms.CountAsync();

            ViewBag.AvailableRooms = await _context.Rooms
                .CountAsync(r => r.IsAvailable);

            ViewBag.TotalBookings = await _context.Bookings.CountAsync();

            ViewBag.TotalUsers = await _userManager.Users.CountAsync();

            ViewBag.TotalRevenue = await _context.Bookings
                .Where(b => b.Status != BookingStatus.Cancelled)
                .SumAsync(b => (decimal?)b.TotalPrice) ?? 0;

            ViewBag.RecentBookings = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .OrderByDescending(b => b.Id)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}