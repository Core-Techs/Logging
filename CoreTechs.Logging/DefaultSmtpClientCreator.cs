using System.Net.Mail;

namespace CoreTechs.Logging
{
    public class DefaultSmtpClientCreator : ICreateSmtpClient
    {
        public SmtpClient CreateSmtpClient()
        {
            return new SmtpClient();
        }
    }
}