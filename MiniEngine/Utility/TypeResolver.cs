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
            typeName = RemoveNullableType(typeName);
            switch (typeName)
            {
                case "System.Single":
                    return float.Parse(value);
                case "System.Double":
                    return double.Parse(value);
                case "System.Byte":
                    return byte.Parse(value);
                case "System.Int16":
                    return short.Parse(value);
                case "System.UInt16":
                    return ushort.Parse(value);
                case "System.Int32":
                    return int.Parse(value);
                case "System.UInt32":
                    return uint.Parse(value);
                case "System.Int64":
                    return long.Parse(value);
                case "System.UInt64":
                    return ulong.Parse(value);
                case "System.Int128":
                    return Int128.Parse(value);
                case "System.UInt128":
                    return UInt128.Parse(value);
                case "System.Boolean":
                    return bool.Parse(value);
                case "System.Char":
                    return char.Parse(value);
                case "System.String":
                    return value;
                case "System.Object":
                    return value;
                case "MiniEngine.MemoryResource":
                    return Resources.GetResource(value);
                case "MiniEngine.Vector2":
                    value = value.Trim('(', ')');
                    var vec2 = value.Split(',', StringSplitOptions.TrimEntries);
                    return new Vector2(int.Parse(vec2[0]), int.Parse(vec2[1]));
                case "MiniEngine.Vector2F":
                    value = value.Trim('(', ')');
                    var vec2F = value.Split(',', StringSplitOptions.TrimEntries);
                    return new Vector2F(float.Parse(vec2F[0]), float.Parse(vec2F[1]));
                case "MiniEngine.Size":
                    value = value.Trim('(', ')');
                    var size = value.Split(',', StringSplitOptions.TrimEntries);
                    return new Size(int.Parse(size[0]), int.Parse(size[1]));
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

        private static string RemoveNullableType(string nullableTypeString)
        {
            var match = NullableRegex().Match(nullableTypeString);
            return match.Success ? match.Groups[1].Value : nullableTypeString;
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly, bool noCache = false)
        {
            if (!noCache && _globalCache.TryGetValue(assembly, out var resultTypes))
                return resultTypes;
            var exportedTypes = assembly.GetTypes();
            _globalCache.Add(assembly, exportedTypes);
            return exportedTypes;
        }

        [GeneratedRegex("^System.Nullable`1\\[\\[(.+), (.+), (.+), (.+), (.+)\\]\\]$")]
        private static partial Regex NullableRegex();
    }
}
