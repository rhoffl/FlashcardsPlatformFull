using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlashcardsPlatformFull.Models;

public class FlashcardProgress
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int FlashcardId { get; set; }

    // SM-2 style SRS fields
    public double EaseFactor { get; set; } = 2.5;
    public int Interval { get; set; }
    public int Repetitions { get; set; }
    public DateTime DueDateUtc { get; set; } = DateTime.UtcNow;

    public DateTime LastReviewedUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public ApplicationUser User { get; set; } = default!;
    public Flashcard Flashcard { get; set; } = default!;
}