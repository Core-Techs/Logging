using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace CoreTechs.Logging
{
    public class DefaultStringConverter : IEntryConverter<string>
    {
        private IFormatException _exceptionFormatter;
        public IFormatException ExceptionFormatter
        {
            get { return _exceptionFormatter ?? (_exceptionFormatter = new DefaultExceptionFormatter()); }
            set { _exceptionFormatter = value; }
        }

        public virtual string Convert([NotNull] LogEntry entry)
        {
            if (entry == null) throw new ArgumentNullException("entry");

            // entry heading
            var sb = new StringBuilder().AppendFormat("{0} : {1} : {2}", entry.Source, entry.Level, entry.Created)
                                        .AppendLine();
            using (var sw = new StringWriter(sb))
            using (var iw = new IndentedTextWriter(sw))
            {
                iw.Indent++;

                // entry message
                var msg = entry.GetMessage();
                if (!msg.IsNullOrWhitespace())
                    iw.WriteLines(msg);

                // entry data
                foreach (var item in entry.Data)
                    iw.WriteLine("{0}: {1}", item.Key, item.Value);

                // exception information
                if (entry.Exception != null)
                    iw.WriteLines(ExceptionFormatter.Format(entry.Exception));
            }

            return sb.AppendLine().ToString();
        }
    }
}