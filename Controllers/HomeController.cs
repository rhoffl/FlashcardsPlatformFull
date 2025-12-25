
using Microsoft.AspNetCore.Mvc;

namespace FlashcardsPlatformFull.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Decks");
}
