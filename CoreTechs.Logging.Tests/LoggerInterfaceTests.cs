using System;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using CoreTechs.Logging.Targets;
using NUnit.Framework;

namespace CoreTechs.Logging.Tests
{
    internal class LoggerInterfaceTests
    {
        private static Logger _log;

        [SetUp]
        public void TestSetup()
        {
            _log = LogManager.Configure("logging").CreateLogger();
            _log.LogManager.UnhandledLoggingException += (s, e) => { throw e.Exception; };
        }

        [Test]
        public void CanFinal()
        {
            using (_log.LogManager)
                _log.Info("test");

            var rams = _log.LogManager.Targets.OfType<MemoryTarget>();
            foreach (var ram in rams)
                Console.WriteLine(ram.View());
        }

        [Test]
        public void WhatsWRongWithThisCOnfig()
        {
            var mgr = new LogManager();
            var mail =
                new EmailTarget
                {
                    To = "roverby@core-techs.net",
                    Subject = "WTF",
                    //Interval = LoggingInterval.Parse("1 minute")
                };

            mgr.Targets.Add(mail);

            var log = mgr.CreateLogger();

            log.Info("test");
            Thread.Sleep(1000);
            mgr.Dispose();	

        }

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

            mgr.Dispose();
        }
        [Test]
        public void BasicEmail()
        {
            var email = new EmailTarget
            {
                From = "logging@example.com",
                To = "roverby@core-techs.net",
                Interval = LoggingInterval.Parse("1 second")
            };

            var mgr = new LogManager(new[] {email});
            mgr.UnhandledLoggingException += (sender, args) => { throw args.Exception; };

            var log = mgr.CreateLogger();

            for (var i = 0; i < 10000; i++)
                log.Info("test");

            mgr.Dispose();
        }

        [Test]
        public void LogToFilesInterval()
        {
            var fileTarget = new FileTarget();
            var mgr = new LogManager(new[] {fileTarget});

            fileTarget.Configure(XElement.Parse(@"<target path=""C:\Users\roverby\Desktop\logtest"" interval=""5 second"" archivecount=""3"" />"));

            Assert.AreEqual(3, fileTarget.ArchiveCount);
            Assert.AreEqual(TimeSpan.FromSeconds(5), fileTarget.Interval.Duration);

            mgr.UnhandledLoggingException += (sender, args) => { throw args.Exception; };
            var log = mgr.CreateLogger();
            for (var i = 0; i < 1000*1000; i++)
            {
                log.Info("test");
                log.Warn("YIKE!");
            }
            
            mgr.Dispose();
        }

        [Test]
        public void LogToFileInDirectory()
        {
            var fileTarget = new FileTarget {Path = @"C:\Users\roverby\Desktop\mylogz.xxx\"};
            var mgr = new LogManager(new[] {fileTarget});
            mgr.UnhandledLoggingException += (sender, args) => { throw args.Exception; };
            var log = mgr.CreateLogger();

            for (var i = 0; i < 10000; i++)
            {
                log.Info("test");
                log.Warn("YIKE!");
            }
            
            mgr.Dispose();
        }

        [Test]
        public void LogToFile()
        {
            var fileTarget = new FileTarget
            {
                Path = @"C:\Users\roverby\Desktop\log.test.txt",
                //Interval= LoggingInterval.Parse("5 second"),
                //ArchiveCount= 3,
                KeepFileOpen = true

            };
            var mgr = new LogManager(new[] {fileTarget});
            mgr.UnhandledLoggingException += (sender, args) => { throw args.Exception; };
            var log = mgr.CreateLogger();

            for (var i = 0; i < 10000; i++)
            {
                log.Info("test");
                log.Warn("YIKE!");
            }
            
            mgr.Dispose();
        }

        [Test]
        public void CanLogToConsole()
        {
            var logmgr = LogManager.Configure("logging");
            logmgr.UnhandledLoggingException += UnhandledException;
            var log = logmgr.CreateLogger();
            log.Data("good", 123).Info();
            logmgr.Dispose();
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
            logmgr.Dispose();
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
            mgr.Dispose();

            Console.WriteLine(memoryTarget.View());
        }
    }
}