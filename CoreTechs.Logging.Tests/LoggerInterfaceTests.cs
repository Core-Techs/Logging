using System;
using System.Threading;
using System.Xml.Linq;
using CoreTechs.Logging.Configuration;
using CoreTechs.Logging.Targets;
using NUnit.Framework;

namespace CoreTechs.Logging.Tests
{
    internal class LoggerInterfaceTests
    {
        private static readonly Logger Log;


        static LoggerInterfaceTests()
        {
            LogManager.Global = LogManager.Configure("logging");
            Log= LogManager.Global.CreateLogger();
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
            mgr.WaitAllWritesComplete();	

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

            fileTarget.Configure(XElement.Parse(@"<target path=""C:\Users\roverby\Desktop\logtest"" interval=""5 second"" archivecount=""2"" />"));

            Assert.AreEqual(2, fileTarget.ArchiveCount);
            Assert.AreEqual(TimeSpan.FromSeconds(5), fileTarget.Interval.Duration);

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
        public void LogToFile()
        {
            var fileTarget = new FileTarget
            {
                Path = @"C:\Users\roverby\Desktop\logtest",
                Interval= LoggingInterval.Parse("5 second"),
                ArchiveCount= 3
            };
            var mgr = new LogManager(new[] {fileTarget});
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