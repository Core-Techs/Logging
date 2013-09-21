using System;

namespace CoreTechs.Logging
{
    public interface IFormatException
    {
        string Format(Exception ex);
    }
}