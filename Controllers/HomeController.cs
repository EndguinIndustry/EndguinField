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

    public IActionResult Index()
    {
        string currentUserId = "1";
        var currentUser = _userService.GetUserById(currentUserId);
        ViewBag.CurrentUser = currentUser;
        return View();
    }

    public IActionResult Privacy()
    {
        string currentUserId = "1";
        var currentUser = _userService.GetUserById(currentUserId);
        ViewBag.CurrentUser = currentUser;
        return View();
    }

    public IActionResult Login()
    {
        string currentUserId = "1";
        var currentUser = _userService.GetUserById(currentUserId);
        ViewBag.CurrentUser = currentUser;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        string currentUserId = "1";
        var currentUser = _userService.GetUserById(currentUserId);
        ViewBag.CurrentUser = currentUser;
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
