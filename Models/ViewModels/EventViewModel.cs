namespace EndguinField.Models.ViewModels;

public class EventViewModel
{
	public string Id { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public List<string> Images { get; set; } = new();
	public string BackgroundImage { get; set; } = string.Empty;
	public string Location { get; set; } = string.Empty;
	public DateTime StartTime { get; set; }
	public DateTime EndTime { get; set; }
	public int MaxParticipants { get; set; }
	public List<string> Tags { get; set; } = new();
	public string Status { get; set; } = "Open";
	public string DisplayStatus { get; set; } = "Open";

	public UserSummaryViewModel Owner { get; set; } = null!;
	public List<ParticipationViewModel> Participants { get; set; } = new();
	public List<CommentViewModel> Comments { get; set; } = new();

	public bool IsOwner { get; set; }
	public bool IsJoined { get; set; }
	public bool IsCreateMode { get; set; }
	public bool IsLoggedIn { get; set; }
	public string? UserStatus { get; set; }

	public int ParticipationCount { get; set; }
}

public class UserSummaryViewModel
{
	public string Id { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string Image { get; set; } = string.Empty;
}

public class ParticipationViewModel
{
	public string UserId { get; set; } = string.Empty;
	public string UserName { get; set; } = string.Empty;
	public string UserImage { get; set; } = string.Empty;
	public string Status { get; set; } = string.Empty;
	public DateTime JoinedTime { get; set; }
}

public class CommentViewModel
{
	public string Id { get; set; } = string.Empty;
	public string Content { get; set; } = string.Empty;
	public DateTime TimeStamp { get; set; }
	public UserSummaryViewModel Sender { get; set; } = null!;
	public string? RepliedMessageId { get; set; }
}

public class EventUpdateModel
{
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public string Location { get; set; } = string.Empty;
	public DateTime StartTime { get; set; }
	public DateTime EndTime { get; set; }
	public int MaxParticipants { get; set; }
	public string Status { get; set; } = "Open";
	public List<string> Tags { get; set; } = new();
	public IFormFile? BackgroundImageFile { get; set; }
	public string? BackgroundImage { get; set; }
	public List<IFormFile>? NewImages { get; set; }
	public List<string>? ExistingImages { get; set; }

	public Event ToEventModel(string ownerId)
	{
		return new Event
		{
			Id = Guid.NewGuid().ToString(),
			OwnerId = ownerId,
			Name = Name,
			Description = Description,
			Location = Location,
			StartTime = StartTime,
			EndTime = EndTime,
			MaxParticipants = MaxParticipants,
			Status = Status,
			Participants = new List<string>(),
			Comments = new List<string>(),
			Images = new List<string>(),
			BackgroundImage = BackgroundImage ?? "https://i.redd.it/n30vilg6ap6g1.jpeg",
			Tags = Tags
		};
	}

	public void UpdateEvent(Event oldEventData)
	{
		oldEventData.Name = Name;
		oldEventData.Description = Description;
		oldEventData.Location = Location;
		oldEventData.StartTime = StartTime;
		oldEventData.EndTime = EndTime;
		oldEventData.MaxParticipants = MaxParticipants;
		oldEventData.Status = Status;
		oldEventData.Tags = Tags;
	}
}