using System;
using System.Collections.Generic;
using System.Threading;
using CoreTechs.Logging.Targets;
using NUnit.Framework;

namespace CoreTechs.Logging.Tests
{
    internal class LoggerTests
    {
        [Test]
        public void UnhandledEx()
        {
            var logger = Logger.ForThis();
            var config = logger.Config;

            config.Targets.Add(new ConsoleTarget());
            config.Targets.Add(new DelegateTarget(e => { throw new Exception("BLAH!"); }));

            config.UnhandledLoggingException += (sender, args) => Console.WriteLine(args.Exception);

            logger.Info("CHEECH!").Write();

            Logger.WaitAllWritesComplete();
        }

        [Test]
        public void How()
        {
            //var config = new LoggingConfiguration {Targets = new Target[] {new ColoredConsoleTarget()}};
            var config = Logger.Configure("logging");

            var log = Logger.ForThis(config);

            log.Error("WTF happened?!")
                .Exception(new AbandonedMutexException())
                .Data("Day", DateTime.Now.DayOfWeek)
                .Write();
            log.Info("Now that's cool!").Write();
            log.Warn("I'm cold...{0}", 23984).Data("FART", !false).Write();
            log.Fatal("I want my mommy!!!").Write();

            Logger.WaitAllWritesComplete();
        }

        [Test]
        public void CanFormatCorrectly()
        {
            try
            {
                DoBad();
            }
            catch (Exception ex)
            {
                var fmt = new DefaultStringFormatter();
                
                var logger = Logger.ForThis();
                var le = new LogEntry(logger)
                {
                    Source = "My Happy Test",
                    Level = Level.Info,
                    MessageFormat = "Yo {0}",
                    MessageArgs = new object[] {"Ronnie"},
                    Exception = ex,
                };

                var data = new Dictionary<string, object>
                {
                    {"Name", "Ronnie"},
                    {"Age", 29},
                    {"Birth Date", new DateTime(1984, 5, 10)}
                };

                foreach (var o in data)
                    le.Data.Add(o.Key, o.Value);

                var s = fmt.Format(le);
                Console.WriteLine(s);
            }
        }

        private static void DoBad()
        {
            try
            {
                DoMoreBad();
            }
            catch (Exception ex)
            {
                var e = new ApplicationException("whoza!", ex);
                e.Data[1] = 2;
                throw e;
            }
        }

        private static void DoMoreBad()
        {
            try
            {
                DoEvenMoreBad();
            }
            catch (Exception ex)
            {
                var ex2 = new InvalidCastException("KRIKEY!", ex);
                ex2.Data["CHONG"] = 234;
                throw ex2;
            }
        }

// ReSharper disable once UnusedMethodReturnValue.Local
        private static int DoEvenMoreBad()
        {
// ReSharper disable once ConvertToConstant.Local
            var zero = 1 - 1;
            var yikes = 3/zero;
            return yikes;
        }
    }
}