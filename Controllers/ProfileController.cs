using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EndguinField.Models;

namespace EndguinField.Controllers;

public class ProfileController : Controller
{
    public IActionResult Index()
    {
        var user = new User
        {
            Id = 1,
            Email = "chen.qianyu@example.com",
            Name = "Chen Qianyu",
            Role = "User",
            Bio = "EndguinField is a platform for wiring and dancing with Chen.",
            Image = "https://preview.redd.it/the-origin-of-chen-sticker-v0-aulsmkoceg0g1.png?width=300&format=png&auto=webp&s=9578b04c0a2fa42f1472ed9bb29b07651ee5effd", 
            Contact = "+1234567890",
            JoinWhen = new DateTime(2026, 2, 14),
            Friends = new List<User>(), 
            Enrolls = new List<Enroll>(), 
            FriendRequests = new List<FriendRequest>(),
            BanUntil = null
        };
        
        var viewModel = ProfileViewModel.FromUser(user);
        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}