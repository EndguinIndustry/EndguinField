using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EndguinField.Models;
using EndguinField.Services;

namespace EndguinField.Controllers;

public class ProfileController : Controller
{
    private readonly IUserService _userService;

    public ProfileController(IUserService userService)
    {
        _userService = userService;
    }

    public IActionResult Index()
    {
        int currentUserId = 1; 
        return RedirectToAction("View", new { id = currentUserId });
    }

    [Route("Profile/{id}")]
    [Route("Profile/View/{id}")]
    public IActionResult View(int id)
    {
        var user = _userService.GetUserById(id);
        
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
        var user = _userService.GetUserById(id);
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}