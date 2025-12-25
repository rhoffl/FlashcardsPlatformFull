
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlashcardsPlatformFull.Data;
using FlashcardsPlatformFull.Models;

namespace FlashcardsPlatformFull.Controllers;

public class DecksController : Controller
{
    private readonly FlashcardsDbContext _db;

    public DecksController(FlashcardsDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var decks = await _db.Decks
            .Include(d => d.Flashcards)
            .OrderBy(d => d.Name)
            .ToListAsync();

        return View(decks);
    }

    public async Task<IActionResult> Details(int id)
    {
        var deck = await _db.Decks
            .Include(d => d.Flashcards.OrderBy(f => f.Id))
            .FirstOrDefaultAsync(d => d.Id == id);

        if (deck == null) return NotFound();
        return View(deck);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View(new Deck());

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(Deck deck)
    {
        if (!ModelState.IsValid) return View(deck);
        _db.Decks.Add(deck);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var deck = await _db.Decks.FindAsync(id);
        if (deck == null) return NotFound();
        return View(deck);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Edit(Deck deck)
    {
        if (!ModelState.IsValid) return View(deck);
        _db.Decks.Update(deck);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deck = await _db.Decks.FindAsync(id);
        if (deck == null) return NotFound();
        return View(deck);
    }

    [Authorize(Roles = "Admin")]
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
