using System;

namespace CoreTechs.Logging.Configuration
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class AliasTypeNameAttribute : Attribute
    {
        public string Alias { get; set; }

        public AliasTypeNameAttribute(string alias)
        {
            Alias = alias;
        }
    }
}