using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Xml.Linq;
using CoreTechs.Logging.Configuration;
using NobleTech.Products.Library;

namespace CoreTechs.Logging.Targets
{
    /// <summary>
    /// A logger that will periodically send accumulated log entries via email.
    /// </summary>
    [FriendlyTypeName("PeriodicEmail")]
    public class PeriodicEmailTarget : PeriodicTarget
    {
        private bool _initialPeriodsSet;
        private readonly ReaderWriterLockSlim _tempFileLock = new ReaderWriterLockSlim();
        private string _tempFile;
        private ICreateSmtpClient _smtpClientFactory;
        public ICreateSmtpClient SmtpClientFactory
        {
            get { return _smtpClientFactory ?? (_smtpClientFactory = new DefaultSmtpClientCreator()); }
            set { _smtpClientFactory = value; }
        }

        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }

        public IEntryFormatter<string> EntryFormatter { get; set; }

        public PeriodicEmailTarget()
        {
            _tempFile = Path.GetTempFileName();
        }

        public override void Write(LogEntry entry)
        {
            if (!_initialPeriodsSet)
            {
                SetPeriods();
                _initialPeriodsSet = true;
            }

            using (var lockmgr = new ReaderWriterLockMgr(_tempFileLock))
            {
                lockmgr.EnterReadLock();

                var fmt = EntryFormatter ?? entry.Logger.Config.GetFormatter<string>();
                var msg = fmt.Format(entry);
                File.AppendAllText(_tempFile, msg);
            }
        }

        protected override void OnPeriodEnd()
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
                using (var smtp = SmtpClientFactory.CreateSmtpClient())
                using (var mail = new MailMessage())
                {
                    if (!string.IsNullOrWhiteSpace(From))
                        mail.From = new MailAddress(From);

                    mail.To.Add(To);
                    mail.Subject = Subject ?? string.Format("{0} Log Entries", AppDomain.CurrentDomain.FriendlyName);
                    
                    mail.Body = File.ReadAllText(oldFile);
                    smtp.Send(mail);
                }

                // delete file
                File.Delete(oldFile);
            }

            // prepare for next period
            SetPeriods();
        }

        public override void Configure(XElement xml)
        {
            base.Configure(xml);

            From = xml.GetAttributeValue("from");
            To = xml.GetAttributeValue("to");
            Subject = xml.GetAttributeValue("subject");
            SmtpClientFactory = ConstructOrDefault<ICreateSmtpClient>(xml.GetAttributeValue("SmtpClientFactory"));
        }
    }
}
