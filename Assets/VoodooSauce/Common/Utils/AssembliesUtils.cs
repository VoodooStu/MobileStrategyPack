using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Voodoo.Sauce.Internal.Utils
{
    public static class AssembliesUtils
    {
        public static List<T> InstantiateInterfaceImplementations<T>() where T : class
        {
            Type interfaceType = typeof(T);
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => interfaceType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                .Select(type => (T) Activator.CreateInstance(type))
                .ToList();
        }

        public static List<Type> GetTypes(Type toGetType)
        {
            List<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => toGetType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                .ToList();

            types.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));

            return types;
        }
    }
}