using System.Net.Mail;
using System.Xml.Linq;
using CoreTechs.Logging;
using CoreTechs.Logging.Configuration;

namespace CoreTechs.Logging.Targets
{
    [FriendlyTypeName("Email")]
    public class EmailTarget : Target, IConfigurableTarget
    {
        private ICreateSmtpClient _smtpClientFactory;
        public ICreateSmtpClient SmtpClientFactory
        {
            get { return _smtpClientFactory ?? (_smtpClientFactory = new DefaultSmtpClientCreator()); }
            set { _smtpClientFactory = value; }
        }

        public IEntryConverter<string> BodyFormatter { get; set; }
        public IEntryConverter<string> SubjectFormatter { get; set; }

        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }

        public override void Write(LogEntry entry)
        {
            using (var smtp = SmtpClientFactory.CreateSmtpClient())
            using (var mail = new MailMessage())
            {
                mail.To.Add(To);

                if (!From.IsNullOrWhitespace())
                    mail.From = new MailAddress(From);

                var fmt = SubjectFormatter ?? new DefaultEmailSubjectFormatter();
                mail.Subject = Subject ?? fmt.Convert(entry);

                fmt = BodyFormatter ?? entry.Logger.LogManager.GetFormatter<string>();
                mail.Body = fmt.Convert(entry);

                smtp.Send(mail);
            }
        }

        public void Configure(XElement xml)
        {
            From = xml.GetAttributeValue("from");
            To = xml.GetAttributeValue("to");
            Subject = xml.GetAttributeValue("subject");

            SmtpClientFactory = ConstructOrDefault<ICreateSmtpClient>(xml.GetAttributeValue("SmtpClientFactory"));
            BodyFormatter = ConstructOrDefault<IEntryConverter<string>>(xml.GetAttributeValue("BodyFormatter"));
            SubjectFormatter = ConstructOrDefault<IEntryConverter<string>>(xml.GetAttributeValue("SubjectFormatter"));
        }
    }
}