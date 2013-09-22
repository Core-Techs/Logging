using System;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace CoreTechs.Logging.Targets
{
    public class DelegateTarget : Target
    {
        private readonly Action<LogEntry> _writeAction;

        public DelegateTarget([NotNull] Action<LogEntry> writeAction)
        {
            if (writeAction == null) throw new ArgumentNullException("writeAction");
            _writeAction = writeAction;
        }

        public override void Write(LogEntry entry)
        {
            _writeAction(entry);
        }
    }
}
