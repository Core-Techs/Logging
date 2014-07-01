using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CoreTechs.Logging
{
    public static class Extensions
    {
        internal static IEnumerable<string> ReadLines(this string s)
        {
            string line;
            using (var sr = new StringReader(s))
                while ((line = sr.ReadLine()) != null)
                    yield return line;
        }

        internal static IEnumerable<T> AsEnumerable<T>(this IEnumerator<T> enumerator)
        {
            if (enumerator == null) throw new ArgumentNullException("enumerator");

            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }

        internal static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (action == null) throw new ArgumentNullException("action");

            foreach (var item in source)
                action(item);
        }

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

        internal static DateTimeOffset LastInstanceAsOf(this TimeSpan timeSpan, DateTimeOffset dt)
        {
            var epoch = new DateTimeOffset(new DateTime(1, 1, 1));
            var periodsPassed = (dt - epoch).Ticks / timeSpan.Ticks;
            var periodBegin = epoch.AddTicks(timeSpan.Ticks * periodsPassed);
            return periodBegin;
        }

        internal static DateTimeOffset LastInstanceAsOfNow(this TimeSpan timeSpan)
        {
            return timeSpan.LastInstanceAsOf(DateTimeOffset.Now);
        }

        internal static TimeSpan Multiply(this TimeSpan duration, int times)
        {
            return new TimeSpan(duration.Ticks * times);
        }

        internal static FileInfo File(this DirectoryInfo dir, string filename)
        {
            var path = Path.Combine(dir.FullName, filename);
            return new FileInfo(path);
        }

        internal static bool ParseBooleanSetting(string s)
        {
            return
                Attempt.Get(() => new[] {"yes", "true", "1"}.Any(x => x.Equals(s, StringComparison.OrdinalIgnoreCase)))
                    .Value;
        }
    }
}