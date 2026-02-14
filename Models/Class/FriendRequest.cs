public class FriendRequest
    {
        public int Id { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public DateTime RequestedAt { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected
    }