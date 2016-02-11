using System;
using System.Diagnostics;
using System.IO;
using CoreTechs.Logging.Targets;
using NUnit.Framework;

namespace CoreTechs.Logging.Tests
{
    public class LogFileTests
    {
        [Test]
        public void KeepOpenIsDefault()
        {
            using (var lf = CreateLogFile())
                Assert.True(lf.KeepOpen);
        }

        private static LogFile CreateLogFile()
        {
            return new LogFile(Path.GetTempFileName());
        }

        [Test]
        public void LogFileCanBeKeptOpen()
        {
            using (var lf = CreateLogFile())
            {
                lf.KeepOpen = true;

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 100000; i++)
                    lf.Append("hey mom");

                Console.WriteLine(sw.Elapsed);
                Console.WriteLine(lf.FileInfo.FullName);
            }
        }

        [Test]
        public void LogFileCanBeNotKeptOpen()
        {
            using (var lf = CreateLogFile())
            {
                lf.KeepOpen = false;

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 10; i++) 
                    lf.Append("hey mom");


                Console.WriteLine(sw.Elapsed);
            }
        }
    }
}