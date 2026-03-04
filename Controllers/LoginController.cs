using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using EndguinField.Services;
using System.Security.Claims;
using EndguinField.Models;

public class LoginController : Controller
{
    private readonly IUserService _userService;
    public LoginController(IUserService userService)
    {
        _userService = userService;
    }
    public IActionResult Index()
    {
        return View();
    }

    public async Task Login()
    {
        Console.WriteLine("Initiating Google authentication...");
        await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
        {
            RedirectUri = Url.Action("GoogleResponse")
        });
    }

    public async Task<IActionResult> GoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!result.Succeeded)
            return RedirectToAction("Index");

        var externalId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(externalId))
        {
            return RedirectToAction("Index");
        }

        var user = await _userService.GetUserByIdAsync(externalId);
        
        if (user != null && user.BanUntil.HasValue && user.BanUntil > DateTime.Now)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Banned", new { until = user.BanUntil });
        }

        if (user == null)
        {
            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;

            var (success, message) = await _userService.CreateUserAsync(new User
            {
                Id = externalId,
                Email = email ?? string.Empty,
                Name = result.Principal.FindFirst(ClaimTypes.Name)?.Value ?? "User",
                Role = "User",
                JoinWhen = DateTime.UtcNow
            });

            if (!success)
            {
                return RedirectToAction("Index");
            }

        }

        return RedirectToAction("ViewProfile", "Profile", new { id = externalId });


    }

    public IActionResult Banned(DateTime? until)
    {
        ViewBag.Until = until;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}