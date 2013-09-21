using System;

namespace CoreTechs.Logging.Configuration
{
    [Serializable]
    public class LoggingConfigurationException : Exception
    {
        public LoggingConfigurationException() { }
        public LoggingConfigurationException( string message ) : base( message ) { }
        public LoggingConfigurationException( string message, Exception inner ) : base( message, inner ) { }
        protected LoggingConfigurationException( 
            System.Runtime.Serialization.SerializationInfo info, 
            System.Runtime.Serialization.StreamingContext context ) : base( info, context ) { }
    }
}