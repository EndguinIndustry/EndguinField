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

	public EventController(IEventService eventService, IUserService userService, IParticipationService participationService)
	{
		_eventService = eventService;
		_userService = userService;
		_participationService = participationService;
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
	public IActionResult Create([FromBody] Event newEvent)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(new { success = false, message = "Invalid data" });
		}

		try
		{
			newEvent.Id = Guid.NewGuid().ToString();
			newEvent.OwnerId = "1";
			newEvent.Status = "Open";
			newEvent.Participants = new List<string>();
			newEvent.Comments = new List<string>();
			newEvent.Images ??= new List<string>();

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

	[Route("Event/{id}")]
	public IActionResult Index(string id)
	{
		string currentUserId = "1";
		var viewModel = _eventService.GetEventViewModel(id, currentUserId);
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
		string currentUserId = "1";

		var existingParticipation = _participationService.GetParticipation(id, currentUserId);
		if (existingParticipation != null)
		{
			return BadRequest(new { success = false, message = "You have already requested to join or have already joined this event." });
		}

		try
		{
			var participation = _participationService.Join(id, currentUserId);
			return Ok(new { success = true, message = "Successfully requested to join the event.", participation });
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
