using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EndguinField.Models;
using EndguinField.Models.ViewModels;
using EndguinField.Services;
using System.Security.Claims;

namespace EndguinField.Controllers;

public class EventController : Controller
{
	private readonly IEventService _eventService;
	private readonly IUserService _userService;
	private readonly IParticipationService _participationService;
	private readonly S3Service _s3Service;


	public EventController(IEventService eventService, IUserService userService, IParticipationService participationService, S3Service s3Service)
	{
		_eventService = eventService;
		_userService = userService;
		_participationService = participationService;
		_s3Service = s3Service;
	}

	[HttpGet]
	[Route("Event/Create")]
	public async Task<IActionResult> Create()
	{
		var currentUserId = _userService.GetCurrentUserId();
		if (string.IsNullOrEmpty(currentUserId))
		{
			return RedirectToAction("Index", "Login");
		}

		var currentUser = await _userService.GetUserByIdAsync(currentUserId);
		if (currentUser == null)
		{
			return RedirectToAction("Index", "Login");
		}

		var viewModel = new EventViewModel
		{
			Id = string.Empty,
			Name = "New Event",
			Description = string.Empty,
			Location = string.Empty,
			StartTime = DateTime.Now,
			EndTime = DateTime.Now.AddHours(2),
			Owner = currentUser.ToSummaryViewModel(),
			IsOwner = true,
			IsLoggedIn = true,
			IsCreateMode = true,
			BackgroundImage = "https://i.redd.it/n30vilg6ap6g1.jpeg"
		};
		return View("Index", viewModel);
	}

	[Route("Event/{id}")]
	public async Task<IActionResult> Index(string id)
	{
		var currentUserId = _userService.GetCurrentUserId() ?? string.Empty;
		var viewModel = await _eventService.GetEventViewModelAsync(id, currentUserId);
		if (viewModel == null)
		{
			return NotFound();
		}

		viewModel.IsLoggedIn = !string.IsNullOrEmpty(currentUserId);
		return View(viewModel);
	}

	[HttpGet]
	[Route("API/Search/NavbarData")]
	public async Task<IActionResult> GetNavbarSearchData()
	{
		var events = await _eventService.GetAllEventsAsync();
		var allUsers = await _userService.GetAllUsersAsync();

		var eventItems = events
			.Where(eventData => !string.IsNullOrWhiteSpace(eventData.Id) && !string.IsNullOrWhiteSpace(eventData.Name))
			.Select(eventData => new
			{
				id = eventData.Id,
				name = eventData.Name,
				location = eventData.Location ?? string.Empty,
				image = eventData.BackgroundImage ?? string.Empty,
				startTime = eventData.StartTime,
				maxParticipants = eventData.MaxParticipants,
				participantCount = eventData.Participants?.Count ?? 0,
				url = Url.Action("Index", "Event", new { id = eventData.Id }) ?? $"/Event/{eventData.Id}"
			})
			.ToList();

		var userItems = allUsers
			.Where(user =>
				!string.IsNullOrWhiteSpace(user.Id) &&
				!string.IsNullOrWhiteSpace(user.Name))
			.Select(user => new
			{
				id = user.Id,
				name = user.Name,
				image = user.Image ?? string.Empty,
				url = Url.Action("ViewProfile", "Profile", new { id = user.Id }) ?? $"/Profile/{user.Id}"
			})
			.Cast<object>()
			.ToList();

		return Ok(new
		{
			events = eventItems,
			users = userItems
		});
	}


	[HttpPost]
	[Route("API/Event/Create")]
	public async Task<IActionResult> Create([FromForm] EventUpdateModel eventData)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(new { success = false, message = "Invalid data" });
		}

		try
		{
			var currentUserId = _userService.GetCurrentUserId();
			if (string.IsNullOrEmpty(currentUserId)) return Unauthorized(new { success = false, message = "User not authenticated." });
			
			var newEvent = eventData.ToEventModel(currentUserId);

			if (eventData.BackgroundImageFile != null)
			{
				newEvent.BackgroundImage = await _s3Service.UploadFileAsync(eventData.BackgroundImageFile);
			}

			if (eventData.NewImages != null)
			{
				foreach (var file in eventData.NewImages)
				{
					var url = await _s3Service.UploadFileAsync(file);
					newEvent.Images.Add(url);
				}
			}

			var (success, error) = await _eventService.CreateEventAsync(newEvent);
			if (!success)
				return BadRequest(new { success = false, message = error });

			return Ok(new { success = true, redirectUrl = Url.Action("Index", "Event", new { id = newEvent.Id }) });
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { success = false, message = ex.Message });
		}
	}

	[HttpPut]
	[Route("API/Event/Update/{id}")]
	public async Task<IActionResult> Update(string id, [FromForm] EventUpdateModel updatedEvent)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(new { success = false, message = "Invalid data" });
		}

		try
		{
			var existingEvent = await _eventService.GetEventByIdAsync(id);
			if (existingEvent == null)
				return NotFound(new { success = false, message = "Event not found." });

			updatedEvent.UpdateEvent(existingEvent);

			if (updatedEvent.BackgroundImageFile != null)
			{
				existingEvent.BackgroundImage = await _s3Service.UploadFileAsync(updatedEvent.BackgroundImageFile);
			}

			existingEvent.Images = updatedEvent.ExistingImages ?? new List<string>();
			if (updatedEvent.NewImages != null)
			{
				foreach (var file in updatedEvent.NewImages)
				{
					existingEvent.Images.Add(await _s3Service.UploadFileAsync(file));
				}
			}

			var (success, error) = await _eventService.UpdateEventAsync(id, existingEvent);
			if (!success)
				return BadRequest(new { success = false, message = error });

			return Ok(new { success = true, message = "Event updated successfully!" });
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { success = false, message = ex.Message });
		}
	}

	[HttpDelete]
	[Route("API/Event/Delete/{id}")]
	public async Task<IActionResult> Delete(string id)
	{
		try
		{
			var existing = await _eventService.GetEventByIdAsync(id);
			if (existing == null)
				return NotFound(new { success = false, message = "Event not found." });

			var currentUserId = _userService.GetCurrentUserId();
			if (existing.OwnerId != currentUserId)
				return Forbid();

			await _eventService.DeleteEventAsync(id);
			return Ok(new { success = true, message = "Event deleted successfully." });
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { success = false, message = ex.Message });
		}
	}

	[HttpPost]
	[Route("API/Event/Join/{id}")]
	public async Task<IActionResult> Join(string id)
	{
		var currentUserId = _userService.GetCurrentUserId();
		if (string.IsNullOrEmpty(currentUserId)) return Unauthorized(new { success = false, message = "User not authenticated." });
		
		var existingParticipation = await _participationService.GetParticipationAsync(id, currentUserId);
		if (existingParticipation != null)
		{
			return BadRequest(new { success = false, message = "You have already requested to join or have already joined this event." });
		}

		try
		{
			var participation = await _participationService.JoinAsync(id, currentUserId);
			return Ok(new { success = true, message = "Successfully requested to join the event.", participation });
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { success = false, message = ex.Message });
		}
	}

	[HttpPost]
	[Route("API/Event/CancelJoin/{id}")]
	public async Task<IActionResult> CancelJoin(string id)
	{
		var currentUserId = _userService.GetCurrentUserId();
		if (string.IsNullOrEmpty(currentUserId)) return Unauthorized(new { success = false, message = "User not authenticated." });

		try
		{
			var success = await _participationService.CancelParticipationAsync(id, currentUserId);
			if (!success) return BadRequest(new { success = false, message = "Failed to cancel join request." });
			return Ok(new { success = true, message = "Successfully cancelled join request." });
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { success = false, message = ex.Message });
		}
	}

	[HttpPost]
	[Route("API/Event/AcceptJoin/{id}")]
	public async Task<IActionResult> AcceptJoin(string id, [FromQuery] string userId)
	{
		var currentUserId = _userService.GetCurrentUserId();
		if (string.IsNullOrEmpty(currentUserId)) return Unauthorized(new { success = false, message = "User not authenticated." });

		var eventData = await _eventService.GetEventByIdAsync(id);
		if (eventData == null || eventData.OwnerId != currentUserId) return Forbid();

		try
		{
			var success = await _participationService.AcceptParticipationAsync(id, userId);
			if (!success) return BadRequest(new { success = false, message = "Failed to accept join request." });
			return Ok(new { success = true, message = "Successfully accepted join request." });
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { success = false, message = ex.Message });
		}
	}

	[HttpPost]
	[Route("API/Event/DeclineJoin/{id}")]
	public async Task<IActionResult> DeclineJoin(string id, [FromQuery] string userId)
	{
		var currentUserId = _userService.GetCurrentUserId();
		if (string.IsNullOrEmpty(currentUserId)) return Unauthorized(new { success = false, message = "User not authenticated." });

		var eventData = await _eventService.GetEventByIdAsync(id);
		if (eventData == null || eventData.OwnerId != currentUserId) return Forbid();

		try
		{
			var success = await _participationService.RejectParticipationAsync(id, userId);
			if (!success) return BadRequest(new { success = false, message = "Failed to decline join request." });
			return Ok(new { success = true, message = "Successfully declined join request." });
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { success = false, message = ex.Message });
		}
	}

	[HttpDelete]
	[Route("API/Event/RemoveParticipant/{id}")]
	public async Task<IActionResult> RemoveParticipant(string id, [FromQuery] string userId)
	{
		var currentUserId = _userService.GetCurrentUserId();
		if (string.IsNullOrEmpty(currentUserId)) return Unauthorized(new { success = false, message = "User not authenticated." });

		var eventData = await _eventService.GetEventByIdAsync(id);
		if (eventData == null || eventData.OwnerId != currentUserId) return Forbid();

		try
		{
			var success = await _participationService.CancelParticipationAsync(id, userId);
			if (!success) return BadRequest(new { success = false, message = "Failed to remove participant." });
			return Ok(new { success = true, message = "Successfully removed participant." });
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { success = false, message = ex.Message });
		}
	}

	[HttpDelete]
	[Route("API/Event/Quit/{id}")]
	public async Task<IActionResult> Quit(string id)
	{
		var currentUserId = _userService.GetCurrentUserId();
		if (string.IsNullOrEmpty(currentUserId)) return Unauthorized(new { success = false, message = "User not authenticated." });

		try
		{
			var success = await _participationService.CancelParticipationAsync(id, currentUserId);
			if (!success) return BadRequest(new { success = false, message = "Failed to quit event." });
			return Ok(new { success = true, message = "Successfully quit event." });
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { success = false, message = ex.Message });
		}
	}

	[HttpPost]
	[Route("API/Event/Comment/{id}")]
	public async Task<IActionResult> AddComment(string id, [FromBody] Comment request)
	{
		var currentUserId = _userService.GetCurrentUserId();
		if (string.IsNullOrEmpty(currentUserId)) return Unauthorized(new { success = false, message = "User not authenticated." });
		
		if (string.IsNullOrWhiteSpace(request.Content))
			return BadRequest(new { success = false, message = "Comment cannot be empty." });

		try
		{
			var (success, error) = await _eventService.AddCommentAsync(id, currentUserId, request.Content, request.RepliedMessageId);
			if (!success)
				return BadRequest(new { success = false, message = error });

			return Ok(new { success = true, message = "Comment added successfully." });
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { success = false, message = ex.Message });
		}
	}

	[HttpGet]
	[Route("Event/CommentsSection/{id}")]
	public async Task<IActionResult> CommentsSection(string id)
	{
		var currentUserId = _userService.GetCurrentUserId() ?? string.Empty;
		var viewModel = await _eventService.GetEventViewModelAsync(id, currentUserId);
		if (viewModel == null)
		{
			return NotFound();
		}
		
		viewModel.IsLoggedIn = !string.IsNullOrEmpty(currentUserId);
		return PartialView("_CommentsSection", viewModel);
	}

	public IActionResult Privacy()
	{
		return View();
	}

	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error()
	{
		return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
	}
}
