using System;
using System.Linq;

namespace CoreTechs.Logging.Configuration
{
    public class TargetTypeInfo
    {
        public Type TargetType { get; set; }
        public AliasTypeNameAttribute FriendlyTypeNameAttribute { get; set; }

        public TargetTypeInfo(Type targetType)
        {
            TargetType = targetType;
            var attrs = targetType.GetCustomAttributes(false).ToArray();
            FriendlyTypeNameAttribute = attrs.OfType<AliasTypeNameAttribute>().SingleOrDefault();
        }
    }
}