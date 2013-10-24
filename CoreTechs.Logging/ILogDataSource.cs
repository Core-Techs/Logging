using System.Collections.Generic;

namespace CoreTechs.Logging
{
    public interface ILogDataSource
    {
        IEnumerable<KeyValuePair<string, object>> GetLogData();
    }
}