using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CoreTechs.Logging
{
    public class DefaultStringConverter : IEntryConverter<string>
    {
        public bool OmitHeading { get; set; }
        public bool Indent { get; set; }

        public DefaultStringConverter()
        {
            OmitHeading = false;
            Indent = true;
        }

        private IFormat<Exception> _exceptionFormatter;
        public IFormat<Exception> ExceptionFormatter
        {
            get { return _exceptionFormatter ?? (_exceptionFormatter = new DefaultExceptionFormatter()); }
            set { _exceptionFormatter = value; }
        }

        public virtual string Convert([NotNull] LogEntry entry)
        {
            if (entry == null) throw new ArgumentNullException("entry");

            // entry heading
            var sb = new StringBuilder();
            if (!OmitHeading)
                sb.AppendFormat("{0} : {1} : {2}", entry.Source, entry.Level, entry.Created).AppendLine();

            using (var sw = new StringWriter(sb))
            using (var iw = new IndentedTextWriter(sw))
            {
                if (Indent) iw.Indent++;

                // entry message
                var msg = entry.GetMessage();
                if (!msg.IsNullOrWhitespace())
                    iw.WriteLines(msg);

                // entry data
                foreach (var item in entry.Data)
                {
                    // first line is written along with key
                    var lines = item.Value.ToString().ReadLines().GetEnumerator().AsEnumerable();
                    iw.WriteLine(item.Key + ": " + lines.FirstOrDefault());

                    // remaining lines are indented
                    iw.Indent++;
                    foreach (var line in lines)
                    {
                        iw.WriteLine(line);
                    }
                    iw.Indent--;
                }

                // exception information
                if (entry.Exception != null)
                    iw.WriteLines(ExceptionFormatter.Format(entry.Exception));
            }

            return sb.AppendLine().ToString();
        }
    }
}