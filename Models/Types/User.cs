namespace EndguinField.Models;

public class User
{
	public int id { get; set; }
	public string email { get; set; }
	public string name { get; set; }
	public string role { get; set; }
	public string bio { get; set; }
	public string image { get; set; }
	public string contact { get; set; }
	public DateTime joinWhen { get; set; }
	public List<string> friends { get; set; }
	public List<string> blockLists { get; set; }
	public List<string> enrolls { get; set; }
	public DateTime banUntil { get; set; }
	public List<string> friendRequests { get; set; }
}