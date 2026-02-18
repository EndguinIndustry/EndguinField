using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EndguinField.Models;

using EndguinField.Models.ViewModels;
using EndguinField.Services;

namespace EndguinField.Controllers;

public class EventDetailController : Controller
{
	private readonly IEventService _eventService;
	private readonly IUserService _userService;
	private readonly IParticipationService _participationService;

	public EventDetailController(IEventService eventService, IUserService userService, IParticipationService participationService)
	{
		_eventService = eventService;
		_userService = userService;
		_participationService = participationService;
	}

	public IActionResult Index(string id)
	{
		var thisEvent = _eventService.GetEventById(id);
		if (thisEvent == null)
		{
			return NotFound();
		}

		var owner = _userService.GetUserById(thisEvent.OwnerId);
		if (owner == null)
		{
			return NotFound();
		}

		var participations = _participationService.GetParticipationsByEvent(thisEvent, _userService);

		List<Comment> allComments = new List<Comment>();

		var viewModel = new EventDetailViewModel
		{
			EventId = thisEvent.EventId,
			Name = thisEvent.Name,
			Description = thisEvent.Description,
			Images = thisEvent.Images,
			BackgroundImage = thisEvent.BackgroundImage,
			Location = thisEvent.Location,
			StartTime = thisEvent.StartTime,
			EndTime = thisEvent.EndTime,
			MaxParticipants = thisEvent.MaxParticipants,
			Category = thisEvent.Category,
			Owner = new UserSummaryViewModel
			{
				Id = owner.Id,
				Name = owner.Name,
				Image = owner.Image ?? string.Empty
			},
			Participations = participations.Select(p => new ParticipationViewModel
			{
				UserId = p.UserId,
				UserName = p.User.Name,
				UserImage = p.User.Image ?? string.Empty,
				Status = p.Status,
				JoinedTime = p.JoinedTime
			}).ToList(),
			Comments = thisEvent.Comments
				.Select(cid =>
				{
					var commentObj = allComments.FirstOrDefault(c => c.Id == cid);
					if (commentObj == null) return null;
					return new CommentViewModel
					{
						Id = commentObj.Id,
						Content = commentObj.Content,
						TimeStamp = commentObj.TimeStamp,
						Author = new UserSummaryViewModel
						{
							Id = commentObj.Sender.Id,
							Name = commentObj.Sender.Name,
							Image = commentObj.Sender.Image ?? string.Empty
						},
						RepliedMessageId = commentObj.RepliedMessageId
					};
				})
				.Where(x => x != null)
				.ToList()!,
			IsOwner = true,
			IsJoined = false
		};
		return View(viewModel);
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
