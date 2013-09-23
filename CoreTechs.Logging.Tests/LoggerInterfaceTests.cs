using System;
using System.Xml.Linq;
using CoreTechs.Logging.Targets;
using NUnit.Framework;

namespace CoreTechs.Logging.Tests
{
    internal class LoggerInterfaceTests
    {
        [Test]
        public void eventvwr()
        {
            var email = new EventLogTarget()
            {
                EventLogSource = "Poopy"
            };

            var mgr = new LogManager(new[] { email });
            mgr.UnhandledLoggingException += (sender, args) => { throw args.Exception; };

            var log = mgr.CreateLogger();

            for (var i = 0; i <  10; i++)
                log.Info("test");

            mgr.WaitAllWritesComplete();
        }
        [Test]
        public void BasicEmail()
        {
            var email = new EmailTarget
                {
                    From = "logging@example.com",
                    To = "roverby@core-techs.net",
                    Interval = LoggingInterval.Parse("1 minute"),
                    Levels=new []{Level.Warn, }
                };

            var mgr = new LogManager(new[] {email});
            mgr.UnhandledLoggingException += (sender, args) => { throw args.Exception; };

            var log = mgr.CreateLogger();

            for (var i = 0; i < 1000*1000*10; i++)
                log.Info("test");

            mgr.WaitAllWritesComplete();
        }

        [Test]
        public void LogToFilesInterval()
        {
            var fileTarget = new FileTarget();
            var mgr = new LogManager(new[] {fileTarget});

            fileTarget.Configure(XElement.Parse(@"<target path=""c:\logs"" interval=""1 minute"" archivecount=""2"" />"));

            Assert.AreEqual(2, fileTarget.ArchiveCount);
            Assert.AreEqual(TimeSpan.FromMinutes(1), fileTarget.Interval.Duration);

            mgr.UnhandledLoggingException += (sender, args) => { throw args.Exception; };
            var log = mgr.CreateLogger();
            for (var i = 0; i < 99999; i++)
            {
                log.Info("test");
                log.Warn("YIKE!"); 
            }
            
            mgr.WaitAllWritesComplete();
        }

        [Test]
        public void CanLogToConsole()
        {
            var logmgr = LogManager.Configure("logging");
            logmgr.UnhandledLoggingException += UnhandledException;
            var log = logmgr.CreateLogger();
            log.Data("good", 123).Info();
            logmgr.WaitAllWritesComplete();
        }

        private static void UnhandledException(object sender, UnhandledLoggingExceptionEventArgs e)
        {
            throw e.Exception;
        }

        [Test]
        public void CanLogAccordingToAppConfig()
        {
            var logmgr = LogManager.Configure("logging");
            logmgr.UnhandledLoggingException += UnhandledException;
            var log = logmgr.CreateLogger();
            log.Info("test");
            logmgr.WaitAllWritesComplete();
        }

        [Test]
        public void CanLogToRAM()
        {
            var memoryTarget = new MemoryTarget { Capacity = 2 };
            var mgr = new LogManager(new[] { memoryTarget, });
            mgr.UnhandledLoggingException += UnhandledException;
            var log = mgr.CreateLogger();
            log.Info("Test");
            log.Warn("something bad");
            log.Fatal("ALERT!");
            mgr.WaitAllWritesComplete();

            Console.WriteLine(memoryTarget.View());

        }
    }
}