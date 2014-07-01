using System;
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
    }
}