using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EndguinField.Models;

namespace EndguinField.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Card()
    {
        return View(new EventModel { Name = "Let's Make Some Factory", Date = "22 Jan 2022 10:00AM", Location = "Valley IV", ImgSrc = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSfvRlHXGZgw0uObx895FilzamGL7LNWoxr8Q&s"});
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
