using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EndguinField.Models.ViewModels;
using EndguinField.Services;

namespace EndguinField.Controllers;

public class HomeController : Controller
{
    private readonly IUserService _userService;

    public HomeController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> Index()
    {
        string currentUserId = "1";
        var currentUser = await _userService.GetUserByIdAsync(currentUserId);
        ViewBag.CurrentUser = currentUser;
        return View();
    }

    public async Task<IActionResult> Privacy()
    {
        string currentUserId = "1";
        var currentUser = await _userService.GetUserByIdAsync(currentUserId);
        ViewBag.CurrentUser = currentUser;
        return View();
    }

    public async Task<IActionResult> Login()
    {
        string currentUserId = "1";
        var currentUser = await _userService.GetUserByIdAsync(currentUserId);
        ViewBag.CurrentUser = currentUser;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Error()
    {
        string currentUserId = "1";
        var currentUser = await _userService.GetUserByIdAsync(currentUserId);
        ViewBag.CurrentUser = currentUser;
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
