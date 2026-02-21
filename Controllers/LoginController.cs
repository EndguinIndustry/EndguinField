using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
public class LoginController : Controller
{
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
        var claims = result.Principal.Identities.FirstOrDefault()?.Claims.Select(claim => new
        {
            claim.Issuer,
            claim.OriginalIssuer,
            claim.Type,
            claim.Value
        });

        return Json(claims);
    }
}