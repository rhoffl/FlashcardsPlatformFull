
namespace FlashcardsPlatformFull.Models;

public class FlashcardProgress
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = default!;

    public int FlashcardId { get; set; }
    public Flashcard Flashcard { get; set; } = default!;

    public double EaseFactor { get; set; } = 2.5;
    public int IntervalDays { get; set; } = 0;
    public int Repetitions { get; set; } = 0;

    public DateTime DueDateUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastReviewedUtc { get; set; }
}
