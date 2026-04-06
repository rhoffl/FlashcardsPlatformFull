using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlashcardsPlatformFull.Models;

public class Deck
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = "";

    public string? Description { get; set; }

    public bool IsPublic { get; set; } = true;

    // ✅ ADD THIS
    public string? OwnerUserId { get; set; }

    // Optional navigation
    [ForeignKey("OwnerUserId")]
    public ApplicationUser? Owner { get; set; }

    public List<Flashcard> Flashcards { get; set; } = new();
}