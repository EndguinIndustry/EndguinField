using EndguinField.Models;
public class HomeViewModel
{
    public List<EventCardModel> PendingEvents { get; set; } = new List<EventCardModel>();
    public List<EventCardModel> ParticipatingEvents { get; set; } = new List<EventCardModel>();
    public List<EventCardModel> OpenEvents { get; set; } = new List<EventCardModel>();
    public ProfileViewModel CurrentUserProfile { get; set; }
    public List<Tag> AllTags { get; set; } = new List<Tag>();
}