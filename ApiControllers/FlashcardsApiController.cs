
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlashcardsPlatformFull.Data;

namespace FlashcardsPlatformFull.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class FlashcardsApiController : ControllerBase
{
    private readonly FlashcardsDbContext _db;
    public FlashcardsApiController(FlashcardsDbContext db) => _db = db;

    [HttpGet("decks")]
    public async Task<IActionResult> Decks()
    {
        var decks = await _db.Decks
            .Where(d => d.IsPublic)
            .OrderBy(d => d.Name)
            .Select(d => new { d.Id, d.Name, d.Description })
            .ToListAsync();
        return Ok(decks);
    }

    [HttpGet("deck/{deckId:int}/cards")]
    public async Task<IActionResult> Cards(int deckId)
    {
        var cards = await _db.Flashcards
            .Where(f => f.DeckId == deckId)
            .OrderBy(f => f.Id)
            .Select(f => new { f.Id, f.Question, f.Answer, f.Category, f.Tags, f.ImagePath, f.AudioPath })
            .ToListAsync();

        return Ok(cards);
    }
}
