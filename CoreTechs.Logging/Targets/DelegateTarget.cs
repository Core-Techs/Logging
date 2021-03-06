﻿using System;

namespace CoreTechs.Logging.Targets
{
    public class DelegateTarget : Target
    {
        private readonly Action<LogEntry> _writeAction;

        public DelegateTarget([NotNull] Action<LogEntry> writeAction)
        {
            if (writeAction == null) throw new ArgumentNullException(nameof(writeAction));
            _writeAction = writeAction;
        }

        public override void Write(LogEntry entry)
        {
            _writeAction(entry);
        }
    }
}
