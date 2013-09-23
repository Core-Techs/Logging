using System;
using System.Xml.Linq;

namespace CoreTechs.Logging.Targets
{
    public class LogEntryBuffer : IConfigurable
    {
        

        public void Write(LogEntry entry)
        {

        }

        public void Configure(XElement xml)
        {
            throw new NotImplementedException();
        }
    }
}