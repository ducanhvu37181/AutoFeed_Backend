using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Services.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordAsync(string toEmail, string fullName, string plainPassword);
    }
}
