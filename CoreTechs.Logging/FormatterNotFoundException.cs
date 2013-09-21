using System;
using System.Runtime.Serialization;

namespace CoreTechs.Logging
{
    [Serializable]
    public class FormatterNotFoundException : LoggingException
    {
        public FormatterNotFoundException()
        {
        }

        public FormatterNotFoundException(string message)
            : base(message)
        {
        }

        public FormatterNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected FormatterNotFoundException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

        public Type FormatterType { get; internal set; }
    }
}