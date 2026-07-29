using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHotelSystem.Data;
using SmartHotelSystem.Models;

namespace SmartHotelSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportsController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalRooms = await _context.Rooms.CountAsync();

            ViewBag.TotalBookings = await _context.Bookings.CountAsync();

            ViewBag.ConfirmedBookings =
                await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Confirmed);

            ViewBag.PendingBookings =
                await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Pending);

            ViewBag.CancelledBookings =
                await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Cancelled);

            ViewBag.TotalRevenue =
                await _context.Bookings
                    .Where(b => b.Status == BookingStatus.Confirmed)
                    .SumAsync(b => (decimal?)b.TotalPrice) ?? 0;

            ViewBag.TotalCustomers =
                await _userManager.Users.CountAsync();

            var mostBookedRoom = await _context.Bookings
                .GroupBy(b => b.RoomId)
                .Select(g => new
                {
                    RoomId = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            if (mostBookedRoom != null)
            {
                ViewBag.MostBookedRoom =
                    await _context.Rooms
                        .Where(r => r.Id == mostBookedRoom.RoomId)
                        .Select(r => r.RoomNumber)
                        .FirstOrDefaultAsync();

                ViewBag.BookingCount = mostBookedRoom.Count;
            }

            return View();
        }
    }
}