
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FlashcardsPlatformFull.Models;

namespace FlashcardsPlatformFull.Data;

public class FlashcardsDbContext : IdentityDbContext<ApplicationUser>
{
    public FlashcardsDbContext(DbContextOptions<FlashcardsDbContext> options) : base(options) { }

    public DbSet<Deck> Decks => Set<Deck>();
    public DbSet<Flashcard> Flashcards => Set<Flashcard>();
    public DbSet<FlashcardProgress> FlashcardProgress => Set<FlashcardProgress>();
    public DbSet<UserStats> UserStats => Set<UserStats>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Deck>(e =>
        {
            e.ToTable("Decks");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.IsPublic).HasDefaultValue(true);
            e.HasIndex(x => x.Name);
        });

        modelBuilder.Entity<Flashcard>(e =>
        {
            e.ToTable("Flashcards");
            e.HasKey(x => x.Id);
            e.Property(x => x.Question).IsRequired().HasMaxLength(500);
            e.Property(x => x.Answer).IsRequired();
            e.Property(x => x.Category).HasMaxLength(100);
            e.Property(x => x.Tags).HasMaxLength(200);
            e.Property(x => x.ImagePath).HasMaxLength(300);
            e.Property(x => x.AudioPath).HasMaxLength(300);

            e.HasOne(x => x.Deck)
                .WithMany(d => d.Flashcards)
                .HasForeignKey(x => x.DeckId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.DeckId, x.Category });
        });

        modelBuilder.Entity<FlashcardProgress>(e =>
        {
            e.ToTable("FlashcardProgress");
            e.HasKey(x => new { x.UserId, x.FlashcardId });

            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Flashcard)
                .WithMany()
                .HasForeignKey(x => x.FlashcardId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Property(x => x.EaseFactor).HasDefaultValue(2.5);
            e.Property(x => x.IntervalDays).HasDefaultValue(0);
            e.Property(x => x.Repetitions).HasDefaultValue(0);
            e.Property(x => x.DueDateUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(x => x.LastReviewedUtc);
            e.HasIndex(x => new { x.UserId, x.DueDateUtc });
        });

        modelBuilder.Entity<UserStats>(e =>
        {
            e.ToTable("UserStats");
            e.HasKey(x => x.UserId);

            e.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<UserStats>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Property(x => x.TotalReviews).HasDefaultValue(0);
            e.Property(x => x.CorrectReviews).HasDefaultValue(0);
            e.Property(x => x.Points).HasDefaultValue(0);
            e.Property(x => x.CurrentStreakDays).HasDefaultValue(0);
            e.Property(x => x.LongestStreakDays).HasDefaultValue(0);
        });
    }
}
