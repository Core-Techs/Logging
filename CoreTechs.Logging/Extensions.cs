using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;

namespace CoreTechs.Logging
{
    public static class Extensions
    {
        public static GenericDisposable<T> AsDisposable<T>(this T obj, Action<T> onDispose)
        {
            return new GenericDisposable<T>(obj, onDispose);
        }

        public static GenericDisposable<ReaderWriterLockSlim> UseReadLock(this ReaderWriterLockSlim @lock)
        {
            @lock.EnterReadLock();
            return @lock.AsDisposable(l => l.ExitReadLock());
        }

        public static GenericDisposable<ReaderWriterLockSlim> UseUpgradeableReadLock(this ReaderWriterLockSlim @lock)
        {
            @lock.EnterUpgradeableReadLock();
            return @lock.AsDisposable(l => l.ExitUpgradeableReadLock());
        }

        public static GenericDisposable<ReaderWriterLockSlim> UseWriteLock(this ReaderWriterLockSlim @lock)
        {
            @lock.EnterWriteLock();
            return @lock.AsDisposable(l => l.ExitWriteLock());
        }

        internal static IEnumerable<string> ReadLines(this string s)
        {
            string line;
            using (var sr = new StringReader(s))
                while ((line = sr.ReadLine()) != null)
                    yield return line;
        }

        internal static IEnumerable<T> AsEnumerable<T>(this IEnumerator<T> enumerator)
        {
            if (enumerator == null) throw new ArgumentNullException(nameof(enumerator));

            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }

        internal static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));

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
            if (name == null) throw new ArgumentNullException(nameof(name));

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

        internal static T Write<T>(this ReaderWriterLockSlim @lock, Func<T> function)
        {
            @lock.EnterWriteLock();
            try
            {
                return function();
            }
            finally
            {
                @lock.ExitWriteLock();
            }
        }

        internal static void Write(this ReaderWriterLockSlim @lock, Action action)
        {
            @lock.Write(() =>
            {
                action();
                return 0;
            });
        }

        internal static T Read<T>(this ReaderWriterLockSlim @lock, Func<T> function)
        {
            @lock.EnterReadLock();
            try
            {
                return function();
            }
            finally
            {
                @lock.ExitReadLock();
            }
        }

        internal static void Read(this ReaderWriterLockSlim @lock, Action action)
        {
            @lock.Read(() =>
            {
                action();
                return 0;
            });
        }

        private static readonly Regex FormatPlaceholderRegex = new Regex(
            @"{(?<index>\d+) *(?:, *(?<align>-?\d+) *)?(?::(?<format>.+?))?}",
            RegexOptions.Compiled);

        private static readonly Regex BraceRegex = new Regex("{|}", RegexOptions.Compiled);

        /// <summary>
        /// Error free string formatting.
        /// Placeholders referencing out of range indexes are removed.
        /// Unescaped braces are automatically escaped.
        /// </summary>
        internal static string SafeFormat(this string format, params object[] args)
        {
            try
            {
                return string.Format(format, args);
            }
            catch (FormatException)
            {
                var sb = new StringBuilder(format);

                // find placeholders
                var matches = FormatPlaceholderRegex.Matches(format).Cast<Match>().ToList();

                var outOfRange = from m in matches
                                 let idx = int.Parse(m.Groups["index"].Value)
                                 where idx >= args.Length
                                 orderby m.Index descending
                                 // ordered from end of string so that we can remove these placeholders
                                 select m;

                var iStale = sb.Length;

                foreach (var match in outOfRange)
                {
                    sb.Remove(match.Index, match.Length);
                    matches.Remove(match);
                    iStale = match.Index;
                }

                var staleMatches = matches.Where(m => m.Index >= iStale).ToArray();
                if (staleMatches.Any())
                {
                    // refresh stale matches
                    // TODO this could probably be avoided by calculating new match positions

                    foreach (var match in staleMatches)
                        matches.Remove(match);

                    var newMatches = FormatPlaceholderRegex
                        .Matches(sb.ToString(), iStale)
                        .Cast<Match>();

                    matches.AddRange(newMatches);
                }

                // need to escape all {} that aren't part of placeholders			

                var matchIntersection = new DelegateEqualityComparer<Match>(
                    (x, y) => SubstringsIntersect(x.Index, x.Length, y.Index, y.Length));

                var needEscaping = BraceRegex.Matches(sb.ToString())
                    .Cast<Match>()
                    .Except(matches, matchIntersection)
                    .OrderByDescending(m => m.Index);

                foreach (var match in needEscaping)
                {
                    sb.Replace(match.Value,
                        match.Value + match.Value,
                        match.Index,
                        match.Length);
                }

                return string.Format(sb.ToString(), args);
            }
        }

        private static bool SubstringsIntersect(int idx1, int len1, int idx2, int len2)
        {
            var a = new {idx = idx1, len = len1};
            var b = new {idx = idx2, len = len2};

            var arr = new[] {a, b}.OrderBy(x => x.idx).ToArray();
            var iMin = arr[0].idx;
            for (var i = 0; i < arr.Length; i++)
                arr[i] = new {idx = arr[i].idx - iMin, arr[i].len};

            return arr[1].idx < arr[0].len;
        }
    }
}