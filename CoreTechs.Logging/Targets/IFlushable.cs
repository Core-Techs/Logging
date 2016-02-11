namespace CoreTechs.Logging.Targets
{
    public interface IFlushable
    {
        void Flush(LogManager logManager);
    }
}