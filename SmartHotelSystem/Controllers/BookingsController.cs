using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartHotelSystem.Data;
using SmartHotelSystem.Models;



[Authorize]

    public class BookingsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingsController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

    // GET: BOOKINGS
    public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var bookings = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Where(b => b.UserId == userId)
                .ToListAsync();

            return View(bookings);
        }

    // GET: BOOKINGS/Details/5
    public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

    // GET: BOOKINGS/Create
    public IActionResult Create(int? roomId)
    {
        ViewData["RoomId"] = new SelectList(
            _context.Rooms.Where(r => r.IsAvailable),
            "Id",
            "RoomNumber",
            roomId);

        return View();
    }

    // POST: BOOKINGS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomId,CheckInDate,CheckOutDate")] Booking booking)
        {
        if (!ModelState.IsValid)
        {
            foreach (var item in ModelState)
            {
                Console.WriteLine($"Field: {item.Key}");

                foreach (var error in item.Value.Errors)
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }
            }

            ViewData["RoomId"] = new SelectList(
                _context.Rooms.Where(r => r.IsAvailable),
                "Id",
                "RoomNumber",
                booking.RoomId);

            return View(booking);
        }

        var room = await _context.Rooms.FindAsync(booking.RoomId);

            if (room == null)
            {
                return NotFound();
            }

            if (booking.CheckOutDate <= booking.CheckInDate)
            {
                ModelState.AddModelError("", "Check-out date must be after check-in date.");

                ViewData["RoomId"] = new SelectList(
                    _context.Rooms.Where(r => r.IsAvailable),
                    "Id",
                    "RoomNumber",
                    booking.RoomId);

                return View(booking);
            }

            // Prevent double booking
            bool roomAlreadyBooked = await _context.Bookings.AnyAsync(b =>
                b.RoomId == booking.RoomId &&
                b.Status != BookingStatus.Cancelled &&
                booking.CheckInDate < b.CheckOutDate &&
                booking.CheckOutDate > b.CheckInDate);

            if (roomAlreadyBooked)
            {
                ModelState.AddModelError("", "This room is already booked for the selected dates.");

                ViewData["RoomId"] = new SelectList(
                    _context.Rooms.Where(r => r.IsAvailable),
                    "Id",
                    "RoomNumber",
                    booking.RoomId);

                return View(booking);
            }

            booking.UserId = _userManager.GetUserId(User)!;
            int nights = (booking.CheckOutDate - booking.CheckInDate).Days;
            booking.TotalPrice = room.PricePerNight * nights;
            booking.Status = BookingStatus.Pending;
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: BOOKINGS/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }

        // POST: BOOKINGS/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,RoomId,Room,UserId,User,CheckInDate,CheckOutDate,TotalPrice,Status")] Booking booking)
        {
            if (id != booking.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            TempData["Success"] = "🎉 Your room has been booked successfully!";
            return RedirectToAction(nameof(Index));
            }
            return View(booking);
        }

        // GET: BOOKINGS/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(m => m.Id == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: BOOKINGS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .OrderByDescending(b => b.Id)
                .ToListAsync();

            return View(bookings);
        }
    // POST: BOOKINGS/Confirm/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Confirm(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);

        if (booking == null)
            return NotFound();

        booking.Status = BookingStatus.Confirmed;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Manage));
    }

    //cancel booking
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Cancel(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);

        if (booking == null)
            return NotFound();

        booking.Status = BookingStatus.Cancelled;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Manage));
    }

    private bool BookingExists(int? id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }
    }
