using CoreTechs.Logging.Configuration;

namespace CoreTechs.Logging.Targets
{
    [AliasTypeName("Null")]
    public class NullTarget : Target
    {
        public override void Write(LogEntry entry)
        {
        }
    }
}