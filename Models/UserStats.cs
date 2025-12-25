
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlashcardsPlatformFull.Models;

public class UserStats
{
    [Key, ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = default!;

    public int TotalReviews { get; set; }
    public int CorrectReviews { get; set; }
    public int Points { get; set; }

    public int CurrentStreakDays { get; set; }
    public int LongestStreakDays { get; set; }
    public DateTime? LastReviewDateUtc { get; set; }
}
