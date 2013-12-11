namespace CoreTechs.Logging
{
    public interface IFormat<in T>
    {
        string Format(T value);
    }
}