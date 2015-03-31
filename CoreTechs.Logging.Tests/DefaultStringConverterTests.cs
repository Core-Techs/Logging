using System;
using System.Threading;
using CoreTechs.Logging.Targets;
using NUnit.Framework;

namespace CoreTechs.Logging.Tests
{
    internal class DefaultStringConverterTests
    {
        [Test]
        public void TestDataIndention()
        {
            var logManager = new LogManager();
            var log = logManager.GetLoggerForCallingType();

            var lb = log.Data("a","happy test").Data("test", "abc\r\nxyz\r\n123").Data("test2", "abc\r\nxyz\r\n123").Data("test3", "abc\r\nxyz\r\n123");

            var s = new DefaultStringConverter().Convert(lb.Entry);

            Console.WriteLine(s);
        }

        [Test]
        public void CanHandleNullDataValue()
        {
            var logManager = new LogManager();
            var log = logManager.GetLoggerForCallingType();
            var lb = log.Data("a", null);
            var s = new DefaultStringConverter().Convert(lb.Entry);
            Assert.True(s.Contains("a: null"));
        }

        [Test]
        public void CanHandleAnonTypes()
        {
            using (var logManager = new LogManager(new[]{new ConsoleTarget()}))
            {
                logManager.UnhandledLoggingException += (s, e) => Console.WriteLine(e.Exception.ToString());


                var log = logManager.GetLoggerForCallingType();
                
                log.Info(new { Abc = 123 });
                log.Info(new { Abc = 123 }.ToString());
                log.Info("{0}", new { Abc = 123 });
            }
        }

        [Test]
        public void CanHandleBadStringFormat()
        {
            Assert.AreEqual("test", "{0}".SafeFormat("test"));
            Assert.AreEqual("test", "test".SafeFormat());
            Assert.AreEqual("test", "test{0}".SafeFormat());
            Assert.AreEqual("test", "{0}{1}".SafeFormat("test"));
            Assert.AreEqual("testtest", "{2}{0}{1}{0}".SafeFormat("test"));
            Assert.AreEqual("{test}", "{{{0}}}".SafeFormat("test"));
            Assert.AreEqual("{0}", "{{0}}".SafeFormat("test"));
            Assert.AreEqual("{abc123}", "{abc123}".SafeFormat());
            Assert.AreEqual("", "{123}".SafeFormat());
            Assert.AreEqual("{abc123}  !", "{abc123} {1} {0}".SafeFormat("!"));
            Assert.AreEqual("{abc123}  {1}", "{abc123} {1} {0}".SafeFormat("{1}"));
        }
    }
}