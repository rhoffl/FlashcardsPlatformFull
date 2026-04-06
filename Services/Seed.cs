using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FlashcardsPlatformFull.Data;
using FlashcardsPlatformFull.Models;

namespace FlashcardsPlatformFull.Services;

public static class Seed
{
    public static async Task SeedRolesAndUsersAsync(IServiceProvider services, IConfiguration cfg)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = { "Admin", "User", "Guest" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Admin
        var adminEmail = cfg["SeedAdmin:Email"] ?? "admin@local";
        var adminPassword = cfg["SeedAdmin:Password"] ?? "Admin123!ChangeMe";
        var adminDisplayName = cfg["SeedAdmin:DisplayName"] ?? "Admin";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = adminDisplayName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (!result.Succeeded)
                throw new Exception("Failed to create admin user: " +
                    string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
            await userManager.AddToRoleAsync(admin, "Admin");

        // Guest
        const string guestEmail = "guest@flashcards.local";
        const string guestPassword = "Guest123!";

        var guest = await userManager.FindByEmailAsync(guestEmail);
        if (guest == null)
        {
            guest = new ApplicationUser
            {
                UserName = guestEmail,
                Email = guestEmail,
                DisplayName = "Guest User",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(guest, guestPassword);
            if (!result.Succeeded)
                throw new Exception("Failed to create guest user: " +
                    string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        if (!await userManager.IsInRoleAsync(guest, "Guest"))
            await userManager.AddToRoleAsync(guest, "Guest");
    }

    public static async Task SeedSampleDataAsync(IServiceProvider sp, IConfiguration cfg)
    {
        var db = sp.GetRequiredService<FlashcardsDbContext>();

        if (await db.Decks.AnyAsync())
            return;

        var starterDeck = new Deck
        {
            Name = "Project Management Basics",
            Description = "Starter deck",
            IsPublic = true
        };

        db.Decks.Add(starterDeck);
        await db.SaveChangesAsync();

        db.Flashcards.AddRange(new[]
        {
            new Flashcard
            {
                DeckId = starterDeck.Id,
                Category = "Core Concepts",
                Tags = "charter,authorization",
                Question = "Which document officially authorizes a project?",
                Answer = "The Project Charter officially authorizes a project."
            },
            new Flashcard
            {
                DeckId = starterDeck.Id,
                Category = "Planning",
                Tags = "wbs",
                Question = "What is a Work Breakdown Structure (WBS)?",
                Answer = "A WBS is a hierarchical decomposition of project work into manageable components."
            }
        });

        await db.SaveChangesAsync();
    }
}