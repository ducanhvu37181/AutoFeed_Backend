namespace AutoFeed_Backend.Models.Requests.User;

public class CreateUserRequest
{
    public int? RoleId { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string Phone { get; set; }
    public string Username { get; set; }
}

public class ChangePasswordRequest
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}
