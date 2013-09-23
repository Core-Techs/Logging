using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace CoreTechs.Logging
{
    public static class Extensions
    {
        internal static bool IsNullOrWhitespace(this string s)
        {
            return String.IsNullOrWhiteSpace(s);
        }

        /// <summary>
        /// Selects an XAttribute by name (case-insensitive) and returns the value or null if an attribute was not found.
        /// </summary>
        public static string GetAttributeValue(this XElement element, [NotNull] string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            var attr =
                element.Attributes()
                    .FirstOrDefault(x => name.Equals(x.Name.ToString(), StringComparison.OrdinalIgnoreCase));

            return attr == null ? null : attr.Value;
        }

        /// <summary>
        /// Selects an XAttribute by name (case-insensitive) and returns the value or null if an attribute was not found.
        /// </summary>
        public static string GetAttributeValue(this XElement element, params string[] names)
        {
            return names.Select(element.GetAttributeValue).FirstOrDefault(a => a != null);
        }

        public static IEnumerable<XElement> Descendants(this XElement element, string name, StringComparison stringComparison)
        {
            return element.Descendants().Where(x => name.Equals(x.Name.ToString(), stringComparison));
        }

        public static DateTimeOffset LastInstanceAsOf(this TimeSpan timeSpan, DateTimeOffset dt)
        {
            var epoch = new DateTimeOffset(new DateTime(1, 1, 1));
            var periodsPassed = (dt - epoch).Ticks / timeSpan.Ticks;
            var periodBegin = epoch.AddTicks(timeSpan.Ticks * periodsPassed);
            return periodBegin;
        }

        public static DateTimeOffset LastInstanceAsOfNow(this TimeSpan timeSpan)
        {
            return timeSpan.LastInstanceAsOf(DateTimeOffset.Now);
        }

        public static TimeSpan Multiply(this TimeSpan duration, int times)
        {
            return new TimeSpan(duration.Ticks * times);
        }

        public static FileInfo File(this DirectoryInfo dir, string filename)
        {
            var path = Path.Combine(dir.FullName, filename);
            return new FileInfo(path);
        }
    }
}