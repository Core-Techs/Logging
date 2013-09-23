using System;
using System.IO;
using System.Net.Mail;
using System.Threading;
using System.Xml.Linq;
using CoreTechs.Logging.Configuration;
using NobleTech.Products.Library;

namespace CoreTechs.Logging.Targets
{
    [AliasTypeName("Email")]
    public class EmailTarget : Target, IConfigurable, IFlushable
    {
        private string _tempFile = Path.GetTempFileName();

        private ICreateSmtpClient _smtpClientFactory;
        private LoggingInterval _interval;
        private readonly ReaderWriterLockSlim _tempFileLock = new ReaderWriterLockSlim();

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

        public LoggingInterval Interval
        {
            get { return _interval; }
            set
            {
                if (_interval != null)
                    _interval.IntervalEnding -= OnIntervalEnding;

                _interval = value;

                if (_interval == null) return;
                _interval.IntervalEnding += OnIntervalEnding;
                _interval.Update();
                _interval.StartTimer();
            }
        }

        private void OnIntervalEnding(object sender, EventArgs eventArgs)
        {
            SendBuffer();
        }

        private void SendBuffer()
        {
            // start logging to a new file
            string oldFile;
            using (var lockmgr = new ReaderWriterLockMgr(_tempFileLock))
            {
                lockmgr.EnterWriteLock();
                oldFile = _tempFile;
                _tempFile = Path.GetTempFileName();
            }

            // email file
            var file = new FileInfo(oldFile);
            if (file.Exists && file.Length > 0)
            {
                var subj = Subject ?? string.Format("{0} Log Entries", AppDomain.CurrentDomain.FriendlyName);
                var body = File.ReadAllText(oldFile);
                Send(subj, body);

                // delete file
                File.Delete(oldFile);
            }

            // prepare for next period
            Interval.Update();
            Interval.StartTimer();
        }

        public override void Write(LogEntry entry)
        {
            if (Interval == null)
                SendEntry(entry);
            else
                BufferEntry(entry);
        }

        private void BufferEntry(LogEntry entry)
        {
            using (var lockmgr = new ReaderWriterLockMgr(_tempFileLock))
            {
                lockmgr.EnterReadLock();

                var body = GetBody(entry);
                File.AppendAllText(_tempFile, body);
            }
        }

        public void SendEntry(LogEntry entry)
        {
            var fmt = SubjectFormatter ?? new DefaultEmailSubjectFormatter();
            var subj = Subject ?? fmt.Convert(entry);
            var body = GetBody(entry);
            Send(subj, body);
        }

        private string GetBody(LogEntry entry)
        {
            var fmt = BodyFormatter ?? entry.Logger.LogManager.GetFormatter<string>();
            var body = fmt.Convert(entry);
            return body;
        }

        public void Send(string subject, string body)
        {
            using (var smtp = SmtpClientFactory.CreateSmtpClient())
            using (var mail = new MailMessage())
            {
                mail.To.Add(To);

                if (!From.IsNullOrWhitespace())
                    mail.From = new MailAddress(From);

                mail.Subject = subject;
                mail.Body = body;

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

            Interval = Try.Get(() => LoggingInterval.Parse(xml.GetAttributeValue("interval"))).Value;
        }

        public void Flush()
        {
            SendBuffer();
        }
    }
}