using System;
using System.Linq;
using System.Xml.Linq;
using CoreTechs.Logging.Targets;
using JetBrains.Annotations;

namespace CoreTechs.Logging.Configuration
{
    public class TargetConstructor
    {
        private readonly TargetTypeInfo[] _targetTypes;

        public TargetConstructor()
        {
            _targetTypes = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where typeof (Target).IsAssignableFrom(type)
                select new TargetTypeInfo(type)).ToArray();
        }

        public Target Construct([NotNull] XElement config)
        {
            if (config == null) throw new ArgumentNullException("config");
            return Construct(null, config);
        }

        public Target Construct(string typeName, XElement config)
        {
            if (config == null) throw new ArgumentNullException("config");
            try
            {
                // make sure that a type name is present (explicitly passed in or through config)
                if (string.IsNullOrWhiteSpace(typeName))
                {
                    typeName = config.GetAttributeValue("type");
                    if (string.IsNullOrWhiteSpace(typeName))
                        throw new ArgumentNullException("typeName");
                }

                // find the target type
                var targetType = FindTargetTypeInfo(typeName);

                if (targetType == null)
                    throw new LoggingConfigurationException("Could not find a target type by the name " + typeName);

                // construct target
                var target = (Target) Activator.CreateInstance(targetType.TargetType);
                target.ConfigureInternal(config);
                return target;
            }
            catch (Exception ex)
            {
                throw new TargetConstructionException("Target construction failed", ex);
            }
        }

        /// <summary>
        /// Searches for the target first by full type name, then by friendly type name, then by type name.
        /// </summary>
        private TargetTypeInfo FindTargetTypeInfo(string typeName)
        {
            // search for target by fullname
            var targetType = (from lt in _targetTypes
                              where lt.TargetType.FullName.Equals(typeName, StringComparison.OrdinalIgnoreCase)
                              select lt).FirstOrDefault();

            if (targetType != null)
                return targetType;

            // search by friendly name
            targetType = (from lt in _targetTypes
                          where lt.FriendlyTypeNameAttribute != null &&
                            lt.FriendlyTypeNameAttribute.FriendlyTypeName.Equals(typeName, StringComparison.OrdinalIgnoreCase)
                          select lt).FirstOrDefault();

            if (targetType != null)
                return targetType;

            // search by name
            targetType = (from lt in _targetTypes
                          where lt.TargetType.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase)
                          select lt).FirstOrDefault();

            if (targetType != null)
                return targetType;

            // search by name starts with
            targetType = (from lt in _targetTypes
                          where lt.TargetType.Name.StartsWith(typeName, StringComparison.OrdinalIgnoreCase)
                          select lt).FirstOrDefault();

            return targetType;
        }
    }
}