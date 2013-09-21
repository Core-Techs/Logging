using System.Net.Mail;
using System.Xml.Linq;
using CoreTechs.Logging.Configuration;

namespace CoreTechs.Logging.Targets
{
    [FriendlyTypeName("Email")]
    public class EmailTarget : Target
    {
        private ICreateSmtpClient _smtpClientFactory;
        public ICreateSmtpClient SmtpClientFactory
        {
            get { return _smtpClientFactory ?? (_smtpClientFactory = new DefaultSmtpClientCreator()); }
            set { _smtpClientFactory = value; }
        }

        public IEntryFormatter<string> BodyFormatter { get; set; }
        public IEntryFormatter<string> SubjectFormatter { get; set; }

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
                mail.Subject = Subject ?? fmt.Format(entry);

                fmt = BodyFormatter ?? entry.Logger.Config.GetFormatter<string>();
                mail.Body = fmt.Format(entry);

                smtp.Send(mail);
            }
        }

        public override void Configure(XElement xml)
        {
            From = xml.GetAttributeValue("from");
            To = xml.GetAttributeValue("to");
            Subject = xml.GetAttributeValue("subject");

            SmtpClientFactory = ConstructOrDefault<ICreateSmtpClient>(xml.GetAttributeValue("SmtpClientFactory"));
            BodyFormatter = ConstructOrDefault<IEntryFormatter<string>>(xml.GetAttributeValue("BodyFormatter"));
            SubjectFormatter = ConstructOrDefault<IEntryFormatter<string>>(xml.GetAttributeValue("SubjectFormatter"));
        }
    }
}
