namespace CoreTechs.Logging
{
    public interface IEntryFormatter
    {
    }

    public interface IEntryFormatter<out TOutput> : IEntryFormatter
    {
        TOutput Format(LogEntry entry);
    }
}