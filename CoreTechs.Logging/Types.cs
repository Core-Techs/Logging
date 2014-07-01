using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CoreTechs.Logging
{
    static class Types
    {
        public static IEnumerable<Type> All(IEnumerable<Assembly> assemblies = null)
        {
            return from a in assemblies ?? GetAssemblies()
                   from t in a.GetTypes()
                   select t;
        }

        public static IEnumerable<Type> Implementing<TParent>(IEnumerable<Assembly> assemblies = null)
        {
            return from t in All(assemblies)
                   where typeof(TParent).IsAssignableFrom(t)
                   select t;
        }

        public static T ConstructOrDefault<T>(this Type type)
        {
            if (type == null) return default(T);
            return (T)Activator.CreateInstance(type);
        }

        public static IEnumerable<Type> Search(this IEnumerable<Type> types, string name,
            StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            return from t in types
                   where t.FullName.Equals(name, stringComparison) || t.Name.Equals(name, stringComparison)
                   select t;
        }

        static IEnumerable<Assembly> GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(a => Attempt.Do(() => a.GetTypes()).Succeeded);
        }

        public static T Construct<T>(string typeName, IEnumerable<Assembly> assemblies = null)
        {
            return All(assemblies).Search(typeName).First().ConstructOrDefault<T>();
        }
    }
}
