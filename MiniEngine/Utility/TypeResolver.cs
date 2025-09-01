using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MiniEngine.Utility
{
    public partial class TypeResolver
    {
        private static readonly Dictionary<Assembly, Type[]> _globalCache = new();
        private static readonly Dictionary<string, Func<string, object>> _additionalParsers = new();
        private readonly Dictionary<string, Type> _localCache = new();
        private readonly List<string> _referenceList = new();

        public void AddReference(string reference)
            => _referenceList.Add(reference);

        public Type? ResolveType(string typeName, bool noCache = false)
        {
            if (!noCache && _localCache.TryGetValue(typeName, out var resultType))
                return resultType;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var type = assemblies.SelectMany(assembly => GetLoadableTypes(assembly))
                .FirstOrDefault(type
                    => type.Name == typeName
                       && (type.Namespace == null || _referenceList.Contains(type.Namespace))
                );
            if (type != null)
                _localCache.Add(typeName, type);

            return type;
        }

        public static object? AutoParse<T>(string value)
            => AutoParse(value, typeof(T));

        public static object? AutoParse(string value, Type type)
        {
            var typeName = type.Namespace != null
                ? $"{type.Namespace}.{type.Name}"
                : type.Name;
            RemoveNullableType(ref typeName);
            var isParsableGeneric = type.GetInterfaces().Any(i => 
                i.IsOfGenericType(typeof(IParsable<>)));
            if (isParsableGeneric)
            {
                try
                {
                    var tryParse = type.GetMethod("Parse", [ typeof(string) ]) ?? type.GetMethod("Parse", [typeof(string), typeof(IFormatProvider)]);
                    var parameters = tryParse?.GetParameters();
                    if (tryParse != null)
                    {
                        var parsed = tryParse.Invoke(null, parameters?.Length > 1
                            ? [value, null]
                            : [value]);
                        if (parsed != null)
                        {
                            LoggingService.Debug("[TypeResolver] Parsed via IParsable<>. Type: {0}, Value: {1}",
                                type.Name, parsed);
                            return parsed;
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }
            switch (typeName)
            {
                case "System.Object":
                    return value;
                case "MiniEngine.MemoryResource":
                    return Resources.GetResource(value);
            }

            return _additionalParsers.TryGetValue(typeName, out var parser)
                ? parser(value)
                : null;
        }

        public static void AddParser<T>(Func<string, object> parserFunc)
            => AddParser(typeof(T), parserFunc);

        public static void AddParser(Type type, Func<string, object> parserFunc)
        {
            var typeName = type.Namespace != null
                ? $"{type.Namespace}.{type.Name}"
                : type.Name;
            AddParser(typeName, parserFunc);
        }

        public static void AddParser(string typeName, Func<string, object> parserFunc)
            => _additionalParsers.Add(typeName, parserFunc);

        private static void RemoveNullableType(ref string nullableTypeString)
        {
            var match = NullableRegex().Match(nullableTypeString);
            nullableTypeString = match.Success ? match.Groups[1].Value : nullableTypeString;
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly, bool noCache = false)
        {
            if (!noCache && _globalCache.TryGetValue(assembly, out var resultTypes))
                return resultTypes;
            var exportedTypes = assembly.GetTypes();
            _globalCache.Add(assembly, exportedTypes);
            return exportedTypes;
        }

        [GeneratedRegex("^System\\.Nullable`1\\[\\[(.+), (.+), (.+), (.+), (.+)\\]\\]$")]
        private static partial Regex NullableRegex();
    }
}
