using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlashcardsPlatformFull.Data;
using FlashcardsPlatformFull.Models;

namespace FlashcardsPlatformFull.Controllers;

public class DecksController : Controller
{
    private readonly FlashcardsDbContext _db;

    public DecksController(FlashcardsDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var isAdmin = User.IsInRole("Admin");
        var userId = User.Identity?.IsAuthenticated == true
            ? User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;

        var query = _db.Decks.Include(d => d.Flashcards).AsQueryable();

        if (isAdmin)
        {
            // all decks
        }
        else if (userId != null)
        {
            query = query.Where(d => d.IsPublic || d.OwnerUserId == userId);
        }
        else
        {
            query = query.Where(d => d.IsPublic);
        }

        var decks = await query.OrderBy(d => d.Name).ToListAsync();
        return View(decks);
    }

    public async Task<IActionResult> Details(int id)
    {
        var deck = await _db.Decks
            .Include(d => d.Flashcards.OrderBy(f => f.Id))
            .FirstOrDefaultAsync(d => d.Id == id);

        if (deck == null)
            return NotFound();

        var isAdmin = User.IsInRole("Admin");
        var userId = User.Identity?.IsAuthenticated == true
            ? User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;

        if (!deck.IsPublic && !(isAdmin || (userId != null && deck.OwnerUserId == userId)))
            return Forbid();

        return View(deck);
    }

    [Authorize(Policy = "AdminOnly")]
    public IActionResult Create() => View(new Deck());

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<IActionResult> Create(Deck deck)
    {
        if (!ModelState.IsValid) return View(deck);

        _db.Decks.Add(deck);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Edit(int id)
    {
        var deck = await _db.Decks.FindAsync(id);
        if (deck == null) return NotFound();
        return View(deck);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<IActionResult> Edit(Deck deck)
    {
        if (!ModelState.IsValid) return View(deck);

        _db.Decks.Update(deck);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var deck = await _db.Decks.FindAsync(id);
        if (deck == null) return NotFound();
        return View(deck);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> ConfirmDelete(int id)
    {
        var deck = await _db.Decks.FindAsync(id);
        if (deck == null) return NotFound();

        _db.Decks.Remove(deck);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}