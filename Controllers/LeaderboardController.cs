using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlashcardsPlatformFull.Data;
using FlashcardsPlatformFull.Models;

namespace FlashcardsPlatformFull.Controllers;

public class LeaderboardController : Controller
{
    private readonly FlashcardsDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public LeaderboardController(
        FlashcardsDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var guestIds = await (
            from userRole in _db.UserRoles
            join role in _db.Roles on userRole.RoleId equals role.Id
            where role.Name == "Guest"
            select userRole.UserId
        ).ToListAsync();

        var top = await _db.UserStats
            .Include(s => s.User)
            .Where(s => !guestIds.Contains(s.UserId))
            .OrderByDescending(s => s.Points)
            .ThenByDescending(s => s.TotalReviews)
            .Take(50)
            .ToListAsync();

        return View(top);
    }
}