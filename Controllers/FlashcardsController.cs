
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlashcardsPlatformFull.Data;
using FlashcardsPlatformFull.Models;
using FlashcardsPlatformFull.Services;

namespace FlashcardsPlatformFull.Controllers;

public class FlashcardsController : Controller
{
    private readonly FlashcardsDbContext _db;
    private readonly IWebHostEnvironment _env;

    public FlashcardsController(FlashcardsDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<IActionResult> Index(int deckId)
    {
        var deck = await _db.Decks.FindAsync(deckId);
        if (deck == null) return NotFound();

        var cards = await _db.Flashcards
            .Where(f => f.DeckId == deckId)
            .OrderBy(f => f.Id)
            .ToListAsync();

        ViewData["Deck"] = deck;
        return View(cards);
    }

    public async Task<IActionResult> Study(int deckId)
    {
        var deck = await _db.Decks.FindAsync(deckId);
        if (deck == null) return NotFound();
        ViewData["Deck"] = deck;
        return View();
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> DueJson(int deckId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        // Due cards are cards whose progress is due OR cards never studied by user
        var now = DateTime.UtcNow;

        var due = await _db.Flashcards
            .Where(f => f.DeckId == deckId)
            .Select(f => new
            {
                Card = f,
                Progress = _db.FlashcardProgress.FirstOrDefault(p => p.UserId == userId && p.FlashcardId == f.Id)
            })
            .ToListAsync();

        var result = due
            .Where(x => x.Progress == null || x.Progress.DueDateUtc <= now)
            .OrderBy(x => x.Progress == null ? DateTime.MinValue : x.Progress.DueDateUtc)
            .Select(x => new
            {
                id = x.Card.Id,
                question = x.Card.Question,
                answer = x.Card.Answer,
                category = x.Card.Category,
                tags = x.Card.Tags,
                imagePath = x.Card.ImagePath,
                audioPath = x.Card.AudioPath
            })
            .ToList();

        return Json(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Review(int deckId, int id, int grade)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var progress = await _db.FlashcardProgress.FindAsync(userId, id);
        if (progress == null)
        {
            progress = new FlashcardProgress
            {
                UserId = userId,
                FlashcardId = id,
                EaseFactor = 2.5,
                IntervalDays = 0,
                Repetitions = 0,
                DueDateUtc = DateTime.UtcNow
            };
            _db.FlashcardProgress.Add(progress);
        }

        var (ef, reps, interval) = Srs.Apply(grade, progress.EaseFactor, progress.Repetitions, progress.IntervalDays);
        progress.EaseFactor = ef;
        progress.Repetitions = reps;
        progress.IntervalDays = interval;
        progress.LastReviewedUtc = DateTime.UtcNow;
        progress.DueDateUtc = DateTime.UtcNow.AddDays(interval);

        await UpdateUserStatsAsync(userId, grade >= 3, grade);

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Study), new { deckId });
    }

    public async Task<IActionResult> Random(int deckId)
    {
        var ids = await _db.Flashcards.Where(f => f.DeckId == deckId).Select(f => f.Id).ToListAsync();
        if (ids.Count == 0) return RedirectToAction(nameof(Index), new { deckId });

        var pickId = ids[new Random().Next(ids.Count)];
        var card = await _db.Flashcards.Include(f => f.Deck).FirstOrDefaultAsync(f => f.Id == pickId);
        if (card == null) return RedirectToAction(nameof(Index), new { deckId });
        return View(card);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(int deckId)
    {
        var deck = await _db.Decks.FindAsync(deckId);
        if (deck == null) return NotFound();
        ViewData["Deck"] = deck;
        return View(new Flashcard { DeckId = deckId });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(Flashcard card, IFormFile? imageFile, IFormFile? audioFile)
    {
        if (!ModelState.IsValid) return View(card);

        await SaveMediaAsync(card, imageFile, audioFile);

        _db.Flashcards.Add(card);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { deckId = card.DeckId });
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var card = await _db.Flashcards.FindAsync(id);
        if (card == null) return NotFound();
        return View(card);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Edit(Flashcard card, IFormFile? imageFile, IFormFile? audioFile)
    {
        if (!ModelState.IsValid) return View(card);

        await SaveMediaAsync(card, imageFile, audioFile, isEdit:true);

        _db.Flashcards.Update(card);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { deckId = card.DeckId });
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var card = await _db.Flashcards.Include(f => f.Deck).FirstOrDefaultAsync(f => f.Id == id);
        if (card == null) return NotFound();
        return View(card);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> ConfirmDelete(int id)
    {
        var card = await _db.Flashcards.FindAsync(id);
        if (card == null) return NotFound();
        var deckId = card.DeckId;
        _db.Flashcards.Remove(card);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { deckId });
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Export(int deckId)
    {
        var cards = await _db.Flashcards.Where(f => f.DeckId == deckId).OrderBy(f => f.Id).ToListAsync();
        var json = JsonSerializer.Serialize(cards.Select(c => new {
            c.Question, c.Answer, c.Category, c.Tags
        }), new JsonSerializerOptions { WriteIndented = true });

        return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", $"deck-{deckId}-flashcards.json");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Import(int deckId)
    {
        ViewData["DeckId"] = deckId;
        return View();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Import(int deckId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ViewData["Error"] = "Select a JSON file.";
            ViewData["DeckId"] = deckId;
            return View();
        }

        using var sr = new StreamReader(file.OpenReadStream());
        var json = await sr.ReadToEndAsync();

        try
        {
            var items = JsonSerializer.Deserialize<List<ImportCard>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            foreach (var i in items)
            {
                _db.Flashcards.Add(new Flashcard
                {
                    DeckId = deckId,
                    Question = i.Question ?? "",
                    Answer = i.Answer ?? "",
                    Category = i.Category ?? "",
                    Tags = i.Tags ?? ""
                });
            }
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { deckId });
        }
        catch (Exception ex)
        {
            ViewData["Error"] = "Import error: " + ex.Message;
            ViewData["DeckId"] = deckId;
            return View();
        }
    }

    private async Task SaveMediaAsync(Flashcard card, IFormFile? imageFile, IFormFile? audioFile, bool isEdit=false)
    {
        var uploads = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploads);

        if (imageFile != null && imageFile.Length > 0)
        {
            var name = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
            var path = Path.Combine(uploads, name);
            using var s = System.IO.File.Create(path);
            await imageFile.CopyToAsync(s);
            card.ImagePath = "/uploads/" + name;
        }

        if (audioFile != null && audioFile.Length > 0)
        {
            var name = Guid.NewGuid() + Path.GetExtension(audioFile.FileName);
            var path = Path.Combine(uploads, name);
            using var s = System.IO.File.Create(path);
            await audioFile.CopyToAsync(s);
            card.AudioPath = "/uploads/" + name;
        }
    }

    private async Task UpdateUserStatsAsync(string userId, bool correct, int grade)
    {
        var stats = await _db.UserStats.FindAsync(userId);
        if (stats == null)
        {
            stats = new UserStats { UserId = userId };
            _db.UserStats.Add(stats);
        }

        stats.TotalReviews += 1;
        if (correct) stats.CorrectReviews += 1;

        // points
        stats.Points += correct ? (grade == 5 ? 15 : 10) : 1;

        // streak
        var today = DateTime.UtcNow.Date;
        if (stats.LastReviewDateUtc?.Date == today)
        {
            // no change
        }
        else if (stats.LastReviewDateUtc?.Date == today.AddDays(-1))
        {
            stats.CurrentStreakDays += 1;
        }
        else
        {
            stats.CurrentStreakDays = 1;
        }

        if (stats.CurrentStreakDays > stats.LongestStreakDays)
            stats.LongestStreakDays = stats.CurrentStreakDays;

        stats.LastReviewDateUtc = DateTime.UtcNow;
    }

    private class ImportCard
    {
        public string? Question { get; set; }
        public string? Answer { get; set; }
        public string? Category { get; set; }
        public string? Tags { get; set; }
    }
}
