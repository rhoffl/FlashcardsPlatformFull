
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlashcardsPlatformFull.Data;

namespace FlashcardsPlatformFull.Controllers;

public class LeaderboardController : Controller
{
    private readonly FlashcardsDbContext _db;
    public LeaderboardController(FlashcardsDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var top = await _db.UserStats
            .Include(s => s.User)
            .OrderByDescending(s => s.Points)
            .ThenByDescending(s => s.TotalReviews)
            .Take(50)
            .ToListAsync();

        return View(top);
    }
}
