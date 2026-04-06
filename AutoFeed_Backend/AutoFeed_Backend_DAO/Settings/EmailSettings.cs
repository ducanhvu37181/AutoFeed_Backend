namespace AutoFeed_Backend_DAO.Settings;

public class EmailSettings
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public string SenderEmail { get; set; } = null!;
    public string SenderName { get; set; } = null!;
    public string Password { get; set; } = null!;
    //public string FrontendUrl { get; set; } = null!;

}