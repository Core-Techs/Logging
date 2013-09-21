namespace CoreTechs.Logging.Targets
{
    internal class DefaultEmailSubjectFormatter : IEntryFormatter<string>
    {
        public string Format(LogEntry entry)
        {
            return string.Format("{0} | {1}", entry.Level, entry.Source);
        }
    }
}