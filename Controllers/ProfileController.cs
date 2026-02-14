using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EndguinField.Models;

namespace EndguinField.Controllers;

public class ProfileController : Controller
{
    public IActionResult Index()
    {
        // Example: This would typically come from a database
        var user = new User
        {
            Id = 1,
            Email = "chen.qianyu@example.com",
            Name = "Chen Qianyu",
            Role = "User",
            Bio = "Passionate about learning and technology. Currently studying Computer Science.",
            Image = "/images/avatar.png", // or null for placeholder
            Contact = "+1234567890",
            JoinWhen = new DateTime(2024, 10, 15),
            Friends = new List<User>(), // Add 5 friends for demo
            Enrolls = new List<Enroll>(), // Add 3 enrollments for demo
            FriendRequests = new List<FriendRequest>(),
            BanUntil = null
        };
        
        for (int i = 0; i < 5; i++)
            user.Friends.Add(new User());
        for (int i = 0; i < 3; i++)
            user.Enrolls.Add(new Enroll());
        
        var viewModel = ProfileViewModel.FromUser(user);
        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}