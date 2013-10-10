﻿using System;
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
        private readonly ReaderWriterLockSlim _tempFileLock = new ReaderWriterLockSlim();
        private LoggingInterval _interval;
        private ICreateSmtpClient _smtpClientFactory;
        private string _tempFile;

        public EmailTarget()
        {
            _tempFile = GetTempFilePath();
        }

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

        private string GetTempFilePath()
        {
            return Path.Combine(Path.GetTempPath(), GetType().FullName, Guid.NewGuid().ToString("n") + ".txt");
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
                _tempFile = GetTempFilePath();
            }

            // email file
            var file = new FileInfo(oldFile);
            if (file.Exists && file.Length > 0)
            {
                string subj = Subject ?? string.Format("{0} Log Entries", AppDomain.CurrentDomain.FriendlyName);
                string body = File.ReadAllText(oldFile);
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

                string body = GetBody(entry);
                File.AppendAllText(_tempFile, body);
            }
        }

        public void SendEntry(LogEntry entry)
        {
            IEntryConverter<string> fmt = SubjectFormatter ?? new DefaultEmailSubjectFormatter();
            string subj = Subject ?? fmt.Convert(entry);
            string body = GetBody(entry);
            Send(subj, body);
        }

        private string GetBody(LogEntry entry)
        {
            IEntryConverter<string> fmt = BodyFormatter ?? entry.Logger.LogManager.GetFormatter<string>();
            string body = fmt.Convert(entry);
            return body;
        }

        public void Send(string subject, string body)
        {
            using (SmtpClient smtp = SmtpClientFactory.CreateSmtpClient())
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
    }
}