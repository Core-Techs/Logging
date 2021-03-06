﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace CoreTechs.Logging.Targets
{
    public class MemoryTarget : Target, IConfigurable
    {
        private readonly Queue<LogEntry> _entries = new Queue<LogEntry>();

        public int? Capacity { get; set; }

        public ReadOnlyCollection<LogEntry> Entries
        {
            get
            {
                return _entries.ToList().AsReadOnly();
            }
        }
        public IEntryConverter<string> EntryFormatter { get; set; }

        public override void Write(LogEntry entry)
        {
            _entries.Enqueue(entry);

            if (_entries.Count > Capacity)
                _entries.Dequeue();
        }

        public string View()
        {
            var conv = EntryFormatter ?? new DefaultStringConverter();
            return string.Join(Environment.NewLine, _entries.Select(conv.Convert));
        }

        public void Clear()
        {
            _entries.Clear();
        }

        public void Configure(XElement xml)
        {
            EntryFormatter =
                ConstructOrDefault<IEntryConverter<string>>(xml.GetAttributeValue("EntryFormatter", "Formatter"));

            var cap = xml.GetAttributeValue("cap") ?? xml.GetAttributeValue("capacity");
            Capacity = Attempt.Get<int?>(() => int.Parse(cap)).Value;
        }
    }
}