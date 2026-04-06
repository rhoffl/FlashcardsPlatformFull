using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FlashcardsPlatformFull.Models;

namespace FlashcardsPlatformFull.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [HttpPost]
    public async Task<IActionResult> GuestLogin()
    {
        await _signInManager.PasswordSignInAsync(
            "guest@flashcards.local",
            "Guest123!",
            isPersistent: false,
            lockoutOnFailure: false
        );

        return RedirectToAction("Index", "Decks");
    }
}