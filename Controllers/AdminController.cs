using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlashcardsPlatformFull.Data;
using FlashcardsPlatformFull.Models;
using FlashcardsPlatformFull.Services;

namespace FlashcardsPlatformFull.Controllers;

[Authorize(Policy = "AdminOnly")]
public class AdminController : Controller
{
    private readonly FlashcardsDbContext _db;
    private readonly IOpenAiFlashcardGenerator _gen;

    public AdminController(
        FlashcardsDbContext db,
        IOpenAiFlashcardGenerator gen)
    {
        _db = db;
        _gen = gen;
    }

    public async Task<IActionResult> Index()
    {
        var guestIds = await (
            from userRole in _db.UserRoles
            join role in _db.Roles on userRole.RoleId equals role.Id
            where role.Name == "Guest"
            select userRole.UserId
        ).ToListAsync();

        var vm = new AdminDashboardVm
        {
            TotalUsers = await _db.Users.CountAsync(),
            TotalDecks = await _db.Decks.CountAsync(),
            TotalCards = await _db.Flashcards.CountAsync(),
            TotalReviews = await _db.UserStats
                .Where(s => !guestIds.Contains(s.UserId))
                .SumAsync(s => (int?)s.TotalReviews) ?? 0,

            TopUsers = await _db.UserStats
                .Include(s => s.User)
                .Where(s => !guestIds.Contains(s.UserId))
                .OrderByDescending(s => s.Points)
                .Take(10)
                .ToListAsync()
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Generate()
    {
        ViewData["Decks"] = await _db.Decks
            .OrderBy(d => d.Name)
            .ToListAsync();

        return View(new GenerateVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(GenerateVm vm)
    {
        ViewData["Decks"] = await _db.Decks
            .OrderBy(d => d.Name)
            .ToListAsync();

        if (vm.DeckId <= 0)
        {
            ViewData["Error"] = "Please select a deck.";
            return View(vm);
        }

        if (string.IsNullOrWhiteSpace(vm.Topic))
        {
            ViewData["Error"] = "Please enter a topic.";
            return View(vm);
        }

        if (vm.Count < 1 || vm.Count > 50)
        {
            ViewData["Error"] = "Count must be between 1 and 50.";
            return View(vm);
        }

        var deck = await _db.Decks.FindAsync(vm.DeckId);
        if (deck == null)
        {
            ViewData["Error"] = "Selected deck was not found.";
            return View(vm);
        }

        var cards = await _gen.GenerateAsync(vm.Topic.Trim(), vm.Count, vm.DeckId);

        if (cards.Count == 0)
        {
            ViewData["Error"] = "No flashcards were generated.";
            return View(vm);
        }

        _db.Flashcards.AddRange(cards);
        await _db.SaveChangesAsync();

        return RedirectToAction("Details", "Decks", new { id = vm.DeckId });
    }

    public class AdminDashboardVm
    {
        public int TotalUsers { get; set; }
        public int TotalDecks { get; set; }
        public int TotalCards { get; set; }
        public int TotalReviews { get; set; }
        public List<UserStats> TopUsers { get; set; } = new();
    }

    public class GenerateVm
    {
        public int DeckId { get; set; }
        public string Topic { get; set; } = string.Empty;
        public int Count { get; set; } = 10;
    }
}