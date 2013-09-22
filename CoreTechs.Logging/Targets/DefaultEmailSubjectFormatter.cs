namespace CoreTechs.Logging.Targets
{
    internal class DefaultEmailSubjectFormatter : IEntryConverter<string>
    {
        public string Convert(LogEntry entry)
        {
            return string.Format("{0}: {1}", entry.Source, entry.Level);
        }
    }
}