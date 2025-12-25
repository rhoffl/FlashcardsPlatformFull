
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlashcardsPlatformFull.Data;
using FlashcardsPlatformFull.Services;

namespace FlashcardsPlatformFull.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly FlashcardsDbContext _db;
    private readonly IOpenAiFlashcardGenerator _gen;

    public AdminController(FlashcardsDbContext db, IOpenAiFlashcardGenerator gen)
    {
        _db = db;
        _gen = gen;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new AdminDashboardVm
        {
            TotalUsers = await _db.Users.CountAsync(),
            TotalDecks = await _db.Decks.CountAsync(),
            TotalCards = await _db.Flashcards.CountAsync(),
            TotalReviews = await _db.UserStats.SumAsync(s => (int?)s.TotalReviews) ?? 0,
            TopUsers = await _db.UserStats.Include(s => s.User).OrderByDescending(s => s.Points).Take(10).ToListAsync()
        };
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Generate()
    {
        ViewData["Decks"] = await _db.Decks.OrderBy(d => d.Name).ToListAsync();
        return View(new GenerateVm());
    }

    [HttpPost]
    public async Task<IActionResult> Generate(GenerateVm vm)
    {
        ViewData["Decks"] = await _db.Decks.OrderBy(d => d.Name).ToListAsync();

        if (vm.DeckId <= 0 || string.IsNullOrWhiteSpace(vm.Topic) || vm.Count < 1 || vm.Count > 50)
        {
            ViewData["Error"] = "Choose a deck, topic, and count (1-50).";
            return View(vm);
        }

        var cards = await _gen.GenerateAsync(vm.Topic.Trim(), vm.Count, vm.DeckId);
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
        public List<FlashcardsPlatformFull.Models.UserStats> TopUsers { get; set; } = new();
    }

    public class GenerateVm
    {
        public int DeckId { get; set; }
        public string Topic { get; set; } = "";
        public int Count { get; set; } = 10;
    }
}
