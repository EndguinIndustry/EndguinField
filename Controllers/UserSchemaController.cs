using Microsoft.AspNetCore.Mvc;

namespace EndguinField.Controllers;

public class UserSchemaController : Controller
{
	private readonly IUserSchemaService _users;

	public UserSchemaController(IUserSchemaService users)
	{
		_users = users;
	}

	public async Task<IActionResult> Index(CancellationToken cancellationToken)
	{
		var allUsers = await _users.GetAllAsync(cancellationToken);
		return View(allUsers);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([FromForm] UserSchema user, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(user.Name))
		{
			TempData["Error"] = "Name is required.";
			return RedirectToAction(nameof(Index));
		}

		await _users.CreateAsync(user, cancellationToken);
		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
	{
		if (!string.IsNullOrWhiteSpace(id))
		{
			await _users.DeleteAsync(id, cancellationToken);
		}

		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> UploadPicture(string id, IFormFile pic, CancellationToken cancellationToken)
	{
		var url = await _users.UploadPictureAsync(id, pic, cancellationToken);
		if (string.IsNullOrWhiteSpace(url))
		{
			TempData["Error"] = "Upload failed. Check user id and selected file.";
		}
		return RedirectToAction(nameof(Index));
	}
}
