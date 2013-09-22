using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace CoreTechs.Logging
{
    public static class Extensions
    {
        internal static bool IsNullOrWhitespace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
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
    }
}