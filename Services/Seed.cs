
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FlashcardsPlatformFull.Data;
using FlashcardsPlatformFull.Models;

namespace FlashcardsPlatformFull.Services;

public static class Seed
{
    public static async Task SeedAdminAsync(IServiceProvider sp, IConfiguration cfg)
    {
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

        var adminRole = "Admin";
        if (!await roleManager.RoleExistsAsync(adminRole))
            await roleManager.CreateAsync(new IdentityRole(adminRole));

        var email = cfg["SeedAdmin:Email"] ?? "admin@local";
        var pwd = cfg["SeedAdmin:Password"] ?? "Admin123!ChangeMe";
        var displayName = cfg["SeedAdmin:DisplayName"] ?? "Admin";

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser { UserName = email, Email = email, DisplayName = displayName, EmailConfirmed = true };
            var create = await userManager.CreateAsync(user, pwd);
            if (!create.Succeeded)
                throw new Exception("Failed to seed admin: " + string.Join("; ", create.Errors.Select(e => e.Description)));
        }

        if (!await userManager.IsInRoleAsync(user, adminRole))
            await userManager.AddToRoleAsync(user, adminRole);
    }

    public static async Task SeedSampleDataAsync(IServiceProvider sp)
    {
        var db = sp.GetRequiredService<FlashcardsDbContext>();
        if (await db.Decks.AnyAsync()) return;

        var deck = new Deck { Name = "Project Management Basics", Description = "Starter deck", IsPublic = true };
        db.Decks.Add(deck);
        await db.SaveChangesAsync();

        db.Flashcards.AddRange(new[]
        {
            new Flashcard { DeckId = deck.Id, Category="Core Concepts", Tags="charter", Question="Which document officially authorizes a project?", Answer="The Project Charter." },
            new Flashcard { DeckId = deck.Id, Category="Planning", Tags="wbs", Question="What is a Work Breakdown Structure (WBS)?", Answer="A hierarchical decomposition of project scope into manageable work packages." },
            new Flashcard { DeckId = deck.Id, Category="Key Concepts", Tags="triple constraint", Question="What are the traditional triple constraints?", Answer="Scope, Time (Schedule), and Cost." },
            new Flashcard { DeckId = deck.Id, Category="Risk", Tags="risk", Question="What is a risk register?", Answer="A document that lists risks, impacts, owners, and response plans." },
            new Flashcard { DeckId = deck.Id, Category="Execution", Tags="stakeholders", Question="Who are stakeholders?", Answer="Individuals or groups who can affect or are affected by the project." }
        });

        await db.SaveChangesAsync();
    }
}
