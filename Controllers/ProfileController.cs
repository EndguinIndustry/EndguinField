using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EndguinField.Models;
using EndguinField.Models.ViewModels;
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
		string currentUserId = "1";
		return RedirectToAction("ViewProfile", new { id = currentUserId });
	}

	[Route("Profile/{id}")]
	[Route("Profile/View/{id}")]
	public async Task<IActionResult> View(string id)
	{
	    var user = await _userService.GetUserByIdAsync(id);
	    if (user == null)
	    {
	        return NotFound();
	    }

	    var viewModel = await ProfileViewModel.FromUserAsync(user, _userService);
	    return View(viewModel);
	}

	[HttpGet]
	[Route("Profile/Edit/{id}")]
	public async Task<IActionResult> Edit(string id)
	{
    var user = await _userService.GetUserByIdAsync(id);
    if (user == null)
    {
        return NotFound();
    }

    return View(user);
}

	[HttpPost]
	[Route("Profile/Edit/{id}")]
	public IActionResult Edit(string id, User updatedUser)
	{
		if (!ModelState.IsValid)
		{
			return View(updatedUser);
		}

		return RedirectToAction("ViewProfile", new { id = id });
	}

	[HttpPost]
	[Route("Profile/Update/{id}")]
	public async Task<IActionResult> Update(string id, [FromForm] UpdateProfileDTO dto, IFormFile? imageFile)
	{
		try
		{
			var user = await _userService.GetUserByIdAsync(id);
			if (user == null)
			{
				return NotFound(new { success = false, message = "User not found" });
			}

			user.Name = dto.Name;
			user.Contact = dto.Contact;
			user.Bio = dto.Bio;

			if (imageFile != null && imageFile.Length > 0)
			{
				var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
				var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

				if (!allowedExtensions.Contains(extension))
				{
					return BadRequest(new { success = false, message = "Invalid file type" });
				}

				if (imageFile.Length > 5 * 1024 * 1024)
				{
					return BadRequest(new { success = false, message = "File size must be less than 5MB" });
				}

				var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
				Directory.CreateDirectory(uploadsFolder);

				var uniqueFileName = $"{id}_{Guid.NewGuid()}{extension}";
				var filePath = Path.Combine(uploadsFolder, uniqueFileName);

				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await imageFile.CopyToAsync(stream);
				}

				user.Image = $"/uploads/profiles/{uniqueFileName}";
			}

			// await _userService.UpdateUserAsync(user);

			return Ok(new
			{
				success = true,
				message = "Profile updated successfully",
				imageUrl = user.Image
			});
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { success = false, message = ex.Message });
		}
	}

	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error()
	{
		return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
	}
}