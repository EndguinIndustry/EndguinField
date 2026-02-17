using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EndguinField.Models;
using EndguinField.Models.ViewModels;

namespace EndguinField.Controllers;

public class EventDetailController : Controller
{
	public IActionResult Index()
	{
		var hostUser = new User
		{
			Id = 1,
			Name = "Endministrator",
			Image = "https://static.zerochan.net/Endministrator.%28Female%29.1024.4086419.webp"
		};

		var participants = new List<User>
		{
			new User { Id = 2, Name = "User1", Image = hostUser.Image },
			new User { Id = 3, Name = "User2", Image = hostUser.Image },
			new User { Id = 4, Name = "User3", Image = hostUser.Image }
		};

		var eventParticipations = new List<Participation>
		{
			new Participation { UserId = 2, EventId = 1, User = participants[0], JoinedTime = DateTime.Now.AddDays(-1), Status = "accepted" },
			new Participation { UserId = 3, EventId = 1, User = participants[1], JoinedTime = DateTime.Now.AddDays(-1), Status = "accepted" },
			new Participation { UserId = 4, EventId = 1, User = participants[2], JoinedTime = DateTime.Now.AddHours(-2), Status = "pending" }
		};

		var thisEvent = new Event
		{
			EventId = "event123",
			OwnerId = hostUser.Id,
			Owner = hostUser,
			Name = "Let's make the factory great again!!!",
			Description = "AHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHH",
			Images = new List<string> {
				"https://static.zerochan.net/Endministrator.%28Female%29.full.4654013.jpg",
				"https://static.zerochan.net/Endministrator.%28Female%29.full.4654013.jpg"
			},
			BackgroundImage = "https://i.redd.it/n30vilg6ap6g1.jpeg",
			StartTime = DateTime.Now,
			EndTime = DateTime.Now.AddHours(2),
			Participants = eventParticipations,
			MaxParticipants = 100,
			Category = "Factory",
			Location = "Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4Valley 4",
			Comments = Enumerable.Range(1, 12).Select(i => new Comment
			{
				Id = i.ToString(),
				Content = $"This is sample comment number {i}. Let's make the factory great again!",
				TimeStamp = DateTime.Now.AddMinutes(-i),
				SenderId = i,
				Sender = new User { Id = i, Name = $"User{i}", Image = hostUser.Image },
				RepliedMessageId = i > 1 && i % 3 == 0 ? (i - 1).ToString() : null
			}).ToList()
		};

		// Manual mapping to ViewModel
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
			MaxEnrolls = thisEvent.MaxParticipants,
			Category = thisEvent.Category,
			Owner = new UserSummaryViewModel
			{
				Id = thisEvent.Owner.Id,
				Name = thisEvent.Owner.Name,
				Image = thisEvent.Owner.Image
			},
			Participations = thisEvent.Participants.Select(p => new ParticipationViewModel
			{
				UserId = p.UserId,
				UserName = p.User.Name,
				UserImage = p.User.Image,
				Status = p.Status,
				JoinedTime = p.JoinedTime
			}).ToList(),
			Comments = thisEvent.Comments.Select(c => new CommentViewModel
			{
				Id = c.Id,
				Content = c.Content,
				TimeStamp = c.TimeStamp,
				Author = new UserSummaryViewModel
				{
					Id = c.Sender.Id,
					Name = c.Sender.Name,
					Image = c.Sender.Image
				},
				RepliedMessageId = c.RepliedMessageId
			}).ToList(),
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
