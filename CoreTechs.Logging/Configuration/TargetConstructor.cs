using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CoreTechs.Logging.Targets;

namespace CoreTechs.Logging.Configuration
{
    public class TargetConstructor
    {
        private readonly TargetTypeInfo[] _targetTypes;

        public TargetConstructor()
        {
            _targetTypes = Types.Implementing<Target>().Select(t => new TargetTypeInfo(t)).ToArray();
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
                var target = (Target)Activator.CreateInstance(targetType.TargetType);
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
            return

                // search by fullname
                _targetTypes.FirstOrDefault(
                    lt => lt.TargetType.FullName.Equals(typeName, StringComparison.OrdinalIgnoreCase))

                // search by friendly type name
                ??
                _targetTypes.FirstOrDefault(
                    lt =>
                    lt.FriendlyTypeNameAttribute != null &&
                    lt.FriendlyTypeNameAttribute.Alias.Equals(typeName, StringComparison.OrdinalIgnoreCase))

                // search by name
                ??
                _targetTypes.FirstOrDefault(
                    lt => lt.TargetType.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase))

                // search by name startswith
                ??
                _targetTypes.FirstOrDefault(
                    lt => lt.TargetType.Name.StartsWith(typeName, StringComparison.OrdinalIgnoreCase));
        }
    }
}