using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EndguinField.Models;

namespace EndguinField.Controllers;

public class ProfileController : Controller
{
    public IActionResult Index()
    {
        int currentUserId = 1; 
        return RedirectToAction("View", new { id = currentUserId });
    }

    [Route("Profile/{id}")]
    [Route("Profile/View/{id}")]
    public IActionResult View(int id)
    {
        var user = GetUserById(id);
        
        if (user == null)
        {
            return NotFound();
        }
        
        var viewModel = ProfileViewModel.FromUser(user);
        return View(viewModel);
    }

    [HttpGet]
    [Route("Profile/Edit/{id}")]
    public IActionResult Edit(int id)
    {
        var user = GetUserById(id);
        if (user == null)
        {
            return NotFound();
        }
        
        return View(user);
    }

    [HttpPost]
    [Route("Profile/Edit/{id}")]
    public IActionResult Edit(int id, User updatedUser)
    {
        if (!ModelState.IsValid)
        {
            return View(updatedUser);
        }
        
        return RedirectToAction("View", new { id = id });
    }

    [Route("Profile/{id}/Friends")]
    public IActionResult Friends(int id)
    {
        var user = GetUserById(id);
        if (user == null)
        {
            return NotFound();
        }
        
        return View(user.Friends);
    }

    [Route("Profile/{id}/Enrollments")]
    public IActionResult Enrollments(int id)
    {
        var user = GetUserById(id);
        if (user == null)
        {
            return NotFound();
        }
        
        return View(user.Enrolls);
    }

    [Route("Profile/FriendRequests")]
    public IActionResult FriendRequests()
    {
        int currentUserId = 1; 
        var user = GetUserById(currentUserId);
        
        if (user == null)
        {
            return NotFound();
        }
        
        return View(user.FriendRequests);
    }

    private User? GetUserById(int id)
    {
        if (id != 1) {
            var notUser1 = new User
            {
                Id = id,
                Email = "ayaka.kamisato@inazuma.com",
                Name = "Ayaka Kamisato",
                Role = "User",
                Bio = $"I am not user number 1, but I am user number: {id} of EndguinField.",
                Image = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSNI8DM4P98gl2Q9AAhD6XlQKS0oaxKsVcJ0w&s", 
                Contact = "+1234567890",
                JoinWhen = new DateTime(2026, 2, 14),
                Friends = new List<User>(), 
                Enrolls = new List<Enroll>(), 
                FriendRequests = new List<FriendRequest>(),
                BanUntil = null
            };

            return notUser1;
        };
        
        var user = new User
        {
            Id = id,
            Email = "chen.qianyu@endguin.com",
            Name = "Chen Qianyu",
            Role = "User",
            Bio = $"EndguinField is a platform for wiring and dancing with Chen. And I am user number: {id} of EndguinField.",
            Image = "https://preview.redd.it/the-origin-of-chen-sticker-v0-aulsmkoceg0g1.png?width=300&format=png&auto=webp&s=9578b04c0a2fa42f1472ed9bb29b07651ee5effd", 
            Contact = "+1234567890",
            JoinWhen = new DateTime(2026, 2, 14),
            Friends = new List<User>(), 
            Enrolls = new List<Enroll>(), 
            FriendRequests = new List<FriendRequest>(),
            BanUntil = null
        };
        
        return user;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}