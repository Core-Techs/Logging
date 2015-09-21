using System;
using System.Net.Mail;

namespace CoreTechs.Logging.Targets
{
    public class DelegateSmtpClientFactory : ICreateSmtpClient
    {
        private readonly Func<SmtpClient> _factory;

        public DelegateSmtpClientFactory(Func<SmtpClient> factory)
        {
            _factory = factory;
        }

        public SmtpClient CreateSmtpClient()
        {
            return _factory();
        }
    }
}