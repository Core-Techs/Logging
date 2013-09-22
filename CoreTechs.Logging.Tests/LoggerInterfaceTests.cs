using System;
using CoreTechs.Logging.Targets;
using NUnit.Framework;

namespace CoreTechs.Logging.Tests
{
    internal class LoggerInterfaceTests
    {
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
            var memoryTarget = new MemoryTarget {Capacity = 2};
            var mgr = new LogManager(new[] {memoryTarget,});
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