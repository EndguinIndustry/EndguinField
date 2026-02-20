using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EndguinField.Models;
using EndguinField.Models.ViewModels;
using EndguinField.Services;

namespace EndguinField.Controllers;

public class EventController : Controller
{
	private readonly IEventService _eventService;
	private readonly IUserService _userService;
	private readonly IParticipationService _participationService;
	private readonly S3Service _s3Service;

	User currentUser = new User // Mocked current user
	{
		Id = "1",
		Name = "Chen Qianyu",
		Image = "https://preview.redd.it/the-origin-of-chen-sticker-v0-aulsmkoceg0g1.png?width=300&format=png&auto=webp&s=9578b04c0a2fa42f1472ed9bb29b07651ee5effd"
	};


	public EventController(IEventService eventService, IUserService userService, IParticipationService participationService, S3Service s3Service)
	{
		_eventService = eventService;
		_userService = userService;
		_participationService = participationService;
		_s3Service = s3Service;
	}

	[HttpGet]
	[Route("Event/Create")]
	public IActionResult Create()
	{
		var viewModel = new EventViewModel
		{
			Id = string.Empty,
			Name = "New Event",
			Description = string.Empty,
			Location = string.Empty,
			StartTime = DateTime.Now,
			EndTime = DateTime.Now.AddHours(2),
			Owner = new UserSummaryViewModel
			{
				Id = "1", // Mocked
				Name = "Chen Qianyu",
				Image = "https://preview.redd.it/the-origin-of-chen-sticker-v0-aulsmkoceg0g1.png?width=300&format=png&auto=webp&s=9578b04c0a2fa42f1472ed9bb29b07651ee5effd"
			},
			IsOwner = true,
			IsCreateMode = true,
			BackgroundImage = "https://i.redd.it/n30vilg6ap6g1.jpeg"
		};
		return View("Index", viewModel);
	}

	[HttpPost]
	[Route("Event/Create")]
	public async Task<IActionResult> Create([FromForm] EventUpdateModel updatedEvent)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(new { success = false, message = "Invalid data" });
		}

		try
		{
			var newEvent = new Event
			{
				Id = Guid.NewGuid().ToString(),
				OwnerId = currentUser.Id,
				Name = updatedEvent.Name,
				Description = updatedEvent.Description,
				Location = updatedEvent.Location,
				StartTime = updatedEvent.StartTime,
				EndTime = updatedEvent.EndTime,
				MaxParticipants = updatedEvent.MaxParticipants,
				Status = "Open",
				Participants = new List<string>(),
				Comments = new List<string>(),
				Images = new List<string>(),
				BackgroundImage = updatedEvent.BackgroundImage ?? "https://i.redd.it/n30vilg6ap6g1.jpeg"
			};

			if (updatedEvent.BackgroundImageFile != null)
			{
				newEvent.BackgroundImage = await _s3Service.UploadFileAsync(updatedEvent.BackgroundImageFile);
			}

			if (updatedEvent.NewImages != null)
			{
				foreach (var file in updatedEvent.NewImages)
				{
					var url = await _s3Service.UploadFileAsync(file);
					newEvent.Images.Add(url);
				}
			}

			var (success, error) = _eventService.CreateEvent(newEvent);
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
	[Route("Event/Update/{id}")]
	public async Task<IActionResult> Update(string id, [FromForm] EventUpdateModel model)
	{
		try
		{
			var existing = _eventService.GetEventById(id);
			if (existing == null)
				return NotFound(new { success = false, message = "Event not found." });

			existing.Name = model.Name;
			existing.Description = model.Description;
			existing.Location = model.Location;
			existing.StartTime = model.StartTime;
			existing.EndTime = model.EndTime;
			existing.MaxParticipants = model.MaxParticipants;
			existing.Status = model.Status;

			if (model.BackgroundImageFile != null)
			{
				existing.BackgroundImage = await _s3Service.UploadFileAsync(model.BackgroundImageFile);
			}

			var finalImages = model.ExistingImages ?? new List<string>();
			if (model.NewImages != null)
			{
				foreach (var file in model.NewImages)
				{
					var url = await _s3Service.UploadFileAsync(file);
					finalImages.Add(url);
				}
			}
			existing.Images = finalImages;

			var (success, error) = _eventService.UpdateEvent(id, existing);
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
	[Route("Event/Delete/{id}")]
	public IActionResult Delete(string id)
	{
		try
		{
			var existing = _eventService.GetEventById(id);
			if (existing == null)
				return NotFound(new { success = false, message = "Event not found." });

			if (existing.OwnerId != currentUser.Id)
				return Forbid();

			_eventService.DeleteEvent(id);
			return Ok(new { success = true, message = "Event deleted successfully." });
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { success = false, message = ex.Message });
		}
	}

	[Route("Event/{id}")]
	public IActionResult Index(string id)
	{
		var viewModel = _eventService.GetEventViewModel(id, currentUser.Id);
		if (viewModel == null)
		{
			return NotFound();
		}

		return View(viewModel);
	}

	[HttpPost]
	[Route("Event/Join/{id}")]
	public IActionResult Join(string id)
	{
		var existingParticipation = _participationService.GetParticipation(id, currentUser.Id);
		if (existingParticipation != null)
		{
			return BadRequest(new { success = false, message = "You have already requested to join or have already joined this event." });
		}

		try
		{
			var participation = _participationService.Join(id, currentUser.Id);
			return Ok(new { success = true, message = "Successfully requested to join the event.", participation });
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { success = false, message = ex.Message });
		}
	}

	[HttpPost]
	[Route("Event/Comment/{id}")]
	public IActionResult AddComment(string id, [FromBody] Comment request)
	{
		if (string.IsNullOrWhiteSpace(request.Content))
			return BadRequest(new { success = false, message = "Comment cannot be empty." });

		try
		{
			var (success, error) = _eventService.AddComment(id, currentUser.Id, request.Content, request.RepliedMessageId);
			if (!success)
				return BadRequest(new { success = false, message = error });

			return Ok(new { success = true, message = "Comment added successfully." });
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { success = false, message = ex.Message });
		}
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
