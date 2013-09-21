using System;

namespace CoreTechs.Logging.Configuration
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class FriendlyTypeNameAttribute : Attribute
    {
        public string FriendlyTypeName { get; set; }

        public FriendlyTypeNameAttribute(string FriendlyTypeName)
        {
            this.FriendlyTypeName = FriendlyTypeName;
        }
    }
}