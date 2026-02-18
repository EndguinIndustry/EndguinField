public class FriendRequest
{
	public string Id { get; set; }
	public string FromUserId { get; set; }
	public string ToUserId { get; set; }
	public DateTime RequestedAt { get; set; }
	public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected
}