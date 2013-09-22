namespace CoreTechs.Logging
{
    public interface IEntryConverter
    {
    }

    public interface IEntryConverter<out TOutput> : IEntryConverter
    {
        TOutput Convert(LogEntry entry);
    }
}