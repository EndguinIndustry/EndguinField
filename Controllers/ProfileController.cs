using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using EndguinField.Models;
using EndguinField.Models.ViewModels;
using EndguinField.Services;

namespace EndguinField.Controllers;

public class ProfileController : Controller
{
	private readonly IUserService _userService;
	private readonly S3Service _s3Service;

	public ProfileController(IUserService userService, S3Service s3Service)
	{
		_userService = userService;
		_s3Service = s3Service;
	}

	public IActionResult Index()
	{
		var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrWhiteSpace(currentUserId))
		{
			return RedirectToAction("Index", "Login");
		}

		return RedirectToAction("ViewProfile", new { id = currentUserId });
	}

	[Route("Profile/{id}")]
	[Route("Profile/View/{id}")]
	public async Task<IActionResult> ViewProfile(string id)
	{
		var user = await _userService.GetUserByIdAsync(id);
		if (user == null)
		{
			return NotFound();
		}

		var viewModel = await ProfileViewModel.FromUserAsync(user, _userService);

		// Check if this is the current user's own profile
		var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		viewModel.IsOwnProfile = !string.IsNullOrWhiteSpace(currentUserId) && currentUserId == id;

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
	public async Task<IActionResult> Edit(string id, User updatedUser)
	{
		if (!ModelState.IsValid)
		{
			return View(updatedUser);
		}

		var (success, error) = await _userService.UpdateUserAsync(id, updatedUser);
		if (!success)
		{
			ModelState.AddModelError(string.Empty, error ?? "Failed to update user");
			return View(updatedUser);
		}

		return RedirectToAction("ViewProfile", new { id = id });
	}

	[HttpPost]
	[Route("Profile/Update/{id}")]
	[Route("API/User/Update/{id}")]
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
			user.Gender = dto.Gender;

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

				user.Image = await _s3Service.UploadFileAsync(imageFile);
			}

			var (success, error) = await _userService.UpdateUserAsync(id, user);
			if (!success)
			{
				return BadRequest(new { success = false, message = error ?? "Failed to update profile" });
			}

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

	[HttpPost]
	[Route("API/AddFriend")]
	public async Task<IActionResult> SendRequest([FromBody] AddFriendRequest request)
	{
		var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);

		if (senderId == request.ReceiverId)
			return BadRequest("Cannot add yourself");

		var receiver = await _userService.GetUserByIdAsync(request.ReceiverId);
		if (receiver == null)
			return NotFound();


		receiver.FriendRequests ??= new List<string>();
		if (!receiver.FriendRequests.Contains(senderId))
		{
			receiver.FriendRequests.Add(senderId);
			await _userService.UpdateUserAsync(request.ReceiverId,receiver);
		}

		return Json(new { success = true });
	}

	[HttpGet]
	[Route("API/User")]
	public async Task<IActionResult> GetAllUsers()
	{
		var users = await _userService.GetAllUsersAsync();
		return Ok(users);
	}

	[HttpGet]
	[Route("API/User/{id}")]
	public async Task<IActionResult> GetUserById(string id)
	{
		var user = await _userService.GetUserByIdAsync(id);
		if (user == null)
		{
			return NotFound(new { success = false, message = "User not found" });
		}

		return Ok(user);
	}

	[HttpDelete]
	[Route("API/User/Delete/{id}")]
	public async Task<IActionResult> DeleteUser(string id)
	{
		var (success, error) = await _userService.DeleteUserAsync(id);
		if (!success)
		{
			return NotFound(new { success = false, message = error ?? "User not found" });
		}

		return Ok(new { success = true, message = "User deleted successfully" });
	}
}