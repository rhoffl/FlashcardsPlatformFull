
using System.ComponentModel.DataAnnotations;

namespace FlashcardsPlatformFull.Models;

public class Flashcard
{
    public int Id { get; set; }

    [Required, StringLength(500)]
    public string Question { get; set; } = string.Empty;

    [Required]
    public string Answer { get; set; } = string.Empty;

    public int DeckId { get; set; }
    public Deck Deck { get; set; } = default!;

    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [StringLength(200)]
    public string Tags { get; set; } = string.Empty;

    // Optional media
    [StringLength(300)]
    public string? ImagePath { get; set; }

    [StringLength(300)]
    public string? AudioPath { get; set; }
}
