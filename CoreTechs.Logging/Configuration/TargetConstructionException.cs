using System;

namespace CoreTechs.Logging.Configuration
{
    [Serializable]
    public class TargetConstructionException : Exception
    {
        public TargetConstructionException() { }
        public TargetConstructionException(string message) : base(message) { }
        public TargetConstructionException(string message, Exception inner) : base(message, inner) { }
        protected TargetConstructionException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}