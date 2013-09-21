using System.Net.Mail;

namespace CoreTechs.Logging
{
    public interface ICreateSmtpClient
    {
        SmtpClient CreateSmtpClient();
    }
}