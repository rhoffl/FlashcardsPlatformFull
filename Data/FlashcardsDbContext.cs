using FlashcardsPlatformFull.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FlashcardsPlatformFull.Data
{
    public class FlashcardsDbContext : IdentityDbContext<ApplicationUser>
    {
        public FlashcardsDbContext(DbContextOptions<FlashcardsDbContext> options)
            : base(options)
        {
        }

        public DbSet<Flashcard> Flashcards => Set<Flashcard>();
        public DbSet<Deck> Decks => Set<Deck>();
        public DbSet<FlashcardProgress> FlashcardProgress => Set<FlashcardProgress>();
        public DbSet<UserStats> UserStats => Set<UserStats>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FlashcardProgress>(entity =>
            {
                entity.HasKey(p => new { p.UserId, p.FlashcardId });

                entity.HasOne(p => p.User)
                      .WithMany()
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Flashcard)
                      .WithMany()
                      .HasForeignKey(p => p.FlashcardId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}