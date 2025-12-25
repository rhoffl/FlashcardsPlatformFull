
using System.ComponentModel.DataAnnotations;

namespace FlashcardsPlatformFull.Models;

public class Deck
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsPublic { get; set; } = true;

    public ICollection<Flashcard> Flashcards { get; set; } = new List<Flashcard>();
}
