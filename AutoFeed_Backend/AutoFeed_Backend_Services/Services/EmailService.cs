using AutoFeed_Backend_DAO.Settings;
using AutoFeed_Backend_Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;


namespace AutoFeed_Backend_Services.Services;
public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendPasswordAsync(string toEmail, string fullName, string plainPassword)
    {
        var message = new MimeMessage();

        // Người gửi
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));

        // Người nhận
        message.To.Add(new MailboxAddress(fullName, toEmail));

        message.Subject = "[AutoFeed] Thông tin đăng nhập hệ thống";

        // Nội dung email
        message.Body = new TextPart("html")
        {
            Text = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto;'>
                    <h2 style='color: #4CAF50;'>Chào mừng đến với AutoFeed!</h2>
                    <p>Xin chào <b>{fullName}</b>,</p>
                    <p>Tài khoản của bạn đã được tạo thành công. Dưới đây là thông tin đăng nhập:</p>
                    <table style='border-collapse: collapse; width: 100%;'>
                        <tr>
                            <td style='padding: 8px; border: 1px solid #ddd;'><b>Email</b></td>
                            <td style='padding: 8px; border: 1px solid #ddd;'>{toEmail}</td>
                        </tr>
                        <tr>
                            <td style='padding: 8px; border: 1px solid #ddd;'><b>Mật khẩu</b></td>
                            <td style='padding: 8px; border: 1px solid #ddd;'>{plainPassword}</td>
                        </tr>
                    </table>
                    <p style='color: red;'>Vui lòng đổi mật khẩu sau khi đăng nhập lần đầu.</p>
                    <p>Trân trọng,<br/><b>AutoFeed System</b></p>
                </div>"
        };
        //message.Body = new TextPart("html")
        //{
        //    Text = $@"
        //<div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto;'>
        //    <h2 style='color: #4CAF50;'>Chào mừng đến với AutoFeed!</h2>
        //    <p>Xin chào <b>{fullName}</b>,</p>
        //    <p>Tài khoản của bạn đã được tạo thành công. Dưới đây là thông tin đăng nhập:</p>
        //    <table style='border-collapse: collapse; width: 100%;'>
        //        <tr>
        //            <td style='padding: 8px; border: 1px solid #ddd;'><b>Email</b></td>
        //            <td style='padding: 8px; border: 1px solid #ddd;'>{toEmail}</td>
        //        </tr>
        //        <tr>
        //            <td style='padding: 8px; border: 1px solid #ddd;'><b>Mật khẩu</b></td>
        //            <td style='padding: 8px; border: 1px solid #ddd;'>{plainPassword}</td>
        //        </tr>
        //    </table>
        //    <br/>
        //    <a href='{_settings.FrontendUrl}' 
        //       style='background-color: #4CAF50; color: white; padding: 12px 24px; 
        //              text-decoration: none; border-radius: 4px; display: inline-block;'>
        //        Đăng nhập hệ thống
        //    </a>
        //    <p style='color: red;'>Vui lòng đổi mật khẩu sau khi đăng nhập lần đầu.</p>
        //    <p>Trân trọng,<br/><b>AutoFeed System</b></p>
        //</div>"
        //};

        using var client = new SmtpClient();

        // Kết nối SMTP
        await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);

        // Đăng nhập
        await client.AuthenticateAsync(_settings.SenderEmail, _settings.Password);

        // Gửi
        await client.SendAsync(message);

        await client.DisconnectAsync(true);
    }
}