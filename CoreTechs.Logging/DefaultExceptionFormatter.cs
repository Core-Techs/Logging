using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace CoreTechs.Logging
{
    class DefaultExceptionFormatter : IFormatException
    {
        public string Format([NotNull] Exception ex)
        {
            if (ex == null) throw new ArgumentNullException("ex");

            var sb = new StringBuilder(ex.ToString());
            using (var sw = new StringWriter(sb))
            using (var iw = new IndentedTextWriter(sw))
            {
                var data = ex.Data.Cast<DictionaryEntry>().ToArray();
                if (data.Any())
                {
                    sb.AppendLine();
                    iw.WriteLine("Exception Data:");
                    iw.Indent++;
                    iw.WriteLines(BuildDataString(data));
                }
            }

            return sb.ToString();
        }

        private static string BuildDataString(IEnumerable<DictionaryEntry> data)
        {
            return string.Join(Environment.NewLine, data.Select(x => string.Format("{0}: {1}", x.Key, x.Value)));
        }
    }
}