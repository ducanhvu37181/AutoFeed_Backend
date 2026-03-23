namespace AutoFeed_Backend.Models.Responses;

public class UserResponse
{
    public int UserId { get; set; }
    public int? RoleId { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string Phone { get; set; }
    public string Username { get; set; }
    public bool? Status { get; set; }
}
