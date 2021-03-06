﻿using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Xml.Linq;
using CoreTechs.Logging.Configuration;

namespace CoreTechs.Logging.Targets
{
    [AliasTypeName("Email")]
    public class EmailTarget : Target, IConfigurable, IFlushable, IDisposable
    {
        private readonly ReaderWriterLockSlim _tempFileLock = new ReaderWriterLockSlim();
        private LoggingInterval _interval;
        private ICreateSmtpClient _smtpClientFactory;
        private LogFile _tempFile;

        public EmailTarget()
        {
            _tempFile = GetTempFile();
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

            var smtpHost = xml.GetAttributeValue("smtpHost", "host");

            if (smtpHost.IsNullOrWhitespace())
            {
                SmtpClientFactory = ConstructOrDefault<ICreateSmtpClient>(xml.GetAttributeValue("SmtpClientFactory"));
            }
            else
            {
                var smtpPort = xml.GetAttributeValue("smtpPort","port");
                var port = smtpPort.IsNullOrWhitespace() ? (int?) null : int.Parse(smtpPort);

                var user = xml.GetAttributeValue("smtpUser", "smtpusername", "username");
                var pw = xml.GetAttributeValue("smtppassword", "password");

                SmtpClientFactory =
                    new DelegateSmtpClientFactory(
                        () =>
                        {
                            var smtpClient = port.HasValue
                                ? new SmtpClient(smtpHost, port.Value)
                                : new SmtpClient(smtpHost);

                            if (!user.IsNullOrWhitespace())
                                smtpClient.Credentials = new NetworkCredential(user, pw);

                            return smtpClient;
                        });
            }

            BodyFormatter = ConstructOrDefault<IEntryConverter<string>>(xml.GetAttributeValue("BodyFormatter"));
            SubjectFormatter = ConstructOrDefault<IEntryConverter<string>>(xml.GetAttributeValue("SubjectFormatter"));

            Interval = Attempt.Get(() => LoggingInterval.Parse(xml.GetAttributeValue("interval"))).Value;
        }

        public void Flush(LogManager logMgr)
        {
            SendBuffer(logMgr);
        }

        private LogFile GetTempFile()
        {
            var path = Path.Combine(Path.GetTempPath(), GetType().FullName, Guid.NewGuid().ToString("n") + ".txt");
            var file = new FileInfo(path);
            if (file.Directory != null && !file.Directory.Exists) file.Directory.Create();
            return new LogFile(file) {DeleteOnDispose = true};
        }

        private void OnIntervalEnding(object sender, EventArgs eventArgs)
        {
            SendBuffer(null /*no log manager access here TODO refactor*/);
        }

        private void SendBuffer(LogManager logMgr)
        {
            // start logging to a new file
            LogFile oldFile;
            using(_tempFileLock.UseWriteLock())
            {
                oldFile = _tempFile;
                _tempFile = GetTempFile();
            }

            using (oldFile)
            {
                // email file
                if (oldFile.FileInfo.Exists && oldFile.FileInfo.Length > 0)
                {
                    var subj = Subject ?? $"{AppDomain.CurrentDomain.FriendlyName} Log Entries";
                    var body = oldFile.ReadAllText();
                    Send(subj, body, logMgr);
                }

                // prepare for next period
                if (Interval == null) return;
                Interval.Update();
                Interval.StartTimer();
            }
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
            using (_tempFileLock.UseReadLock())
            {
                var body = GetBody(entry);
                _tempFile.Append(body);
            }
        }

        public void SendEntry(LogEntry entry)
        {
            var fmt = SubjectFormatter ?? new DefaultEmailSubjectFormatter();
            var subj = Subject ?? fmt.Convert(entry);
            var body = GetBody(entry);
            Send(subj, body, entry.Logger.LogManager);
        }

        private string GetBody(LogEntry entry)
        {
            IEntryConverter<string> fmt = BodyFormatter ?? entry.Logger.LogManager.GetFormatter<string>();
            string body = fmt.Convert(entry);
            return body;
        }

        public void Send(string subject, string body, LogManager logMgr)
        {
            try
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
            catch (Exception ex)
            {
                logMgr?.OnUnhandledLoggingException(ex);
            }
        }

        public void Dispose()
        {
            using (_interval)
            using (_tempFileLock)
            using (_tempFile)
            {

            }
        }
    }
}