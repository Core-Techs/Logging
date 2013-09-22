using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace CoreTechs.Logging
{
    internal static class Enums
    {
        public static IEnumerable<T> GetAll<T>()
        {
            var type = typeof (T);
            if (!type.IsEnum)
                throw new ArgumentException("The type argument must be an enum type");

            return Enum.GetValues(type).Cast<T>();
        }

        public static TEnum Parse<TEnum>([NotNull] string s)
        {
            if (s == null) throw new ArgumentNullException("s");
            var t = typeof (TEnum);
            if (!t.IsEnum)
                throw new InvalidOperationException("TEnum must be an enum type");

            return (TEnum) Enum.Parse(t, s, true);
        }
    }
}