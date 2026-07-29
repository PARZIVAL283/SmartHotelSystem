
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHotelSystem.Models;
using SmartHotelSystem.Data;
using Microsoft.AspNetCore.Authorization;


public class RoomsController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public RoomsController(
    AppDbContext context,
    IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: ROOMS
    public async Task<IActionResult> Index(
    string? search,
    decimal? maxPrice,
    int? capacity,
    bool availableOnly = false)
    {
        var rooms = _context.Rooms.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            rooms = rooms.Where(r =>
                r.RoomNumber.Contains(search) ||
                r.Category.Contains(search));
        }

        if (maxPrice.HasValue)
        {
            rooms = rooms.Where(r => r.PricePerNight <= maxPrice.Value);
        }

        if (capacity.HasValue)
        {
            rooms = rooms.Where(r => r.Capacity >= capacity.Value);
        }

        if (availableOnly)
        {
            rooms = rooms.Where(r => r.IsAvailable);
        }

        return View(await rooms.ToListAsync());
    }

    // GET: ROOMS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var room = await _context.Rooms
            .FirstOrDefaultAsync(m => m.Id == id);
        if (room == null)
        {
            return NotFound();
        }

        return View(room);
    }

    // GET: ROOMS/Create
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: ROOMS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Room room)
    {
        if (ModelState.IsValid)
        {
            if (room.ImageFile != null)
            {
                try
                {
                    if (string.IsNullOrEmpty(_webHostEnvironment.WebRootPath))
                    {
                        return Content("WebRootPath is NULL");
                    }

                    string uploadsFolder = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        "images",
                        "rooms");

                    Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName =
                        Guid.NewGuid().ToString() +
                        Path.GetExtension(room.ImageFile.FileName);

                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await room.ImageFile.CopyToAsync(stream);
                    }

                    room.ImageUrl = "/images/rooms/" + uniqueFileName;
                }
                catch (Exception ex)
                {
                    return Content(ex.ToString());
                }
            }
            _context.Add(room);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(room);
    }

    // GET: ROOMS/Edit/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
        {
            return NotFound();
        }
        return View(room);
    }

    // POST: ROOMS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,RoomNumber,Category,PricePerNight,Capacity,IsAvailable,Description,ImageUrl")] Room room)
    {
        if (id != room.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(room);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(room.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(room);
    }

    // GET: ROOMS/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var room = await _context.Rooms
            .FirstOrDefaultAsync(m => m.Id == id);
        if (room == null)
        {
            return NotFound();
        }

        return View(room);
    }

    // POST: ROOMS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room != null)
        {
            _context.Rooms.Remove(room);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool RoomExists(int? id)
    {
        return _context.Rooms.Any(e => e.Id == id);
    }
}
