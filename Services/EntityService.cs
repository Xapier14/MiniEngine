using MiniEngine.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    public static class EntityService
    {
        private static readonly Dictionary<Type, ConstructorInfo?> _componentConstructorCache = new();

        public static T? TryCreateEntity<T>(params object[] args) where T : Entity
        {
            var entityType = typeof(T);
            if (entityType.IsAssignableFrom(typeof(Entity)))
            {
                LoggingService.Error("Error creating entity {0}, type is not assignable to Entity.", entityType.Name);
                return null;
            }

            var constructors = entityType.GetConstructors();
            var entityCtr = entityType.GetConstructor(Array.Empty<Type>());
            Entity? result = null;
            foreach (var constructor in constructors)
            {
                try
                {
                    var attributes = constructor.GetCustomAttributes()
                        .Where(attribute => attribute.GetType().IsAssignableTo(typeof(IInjectAttribute)));
                    var injectTypes = attributes.Cast<IInjectAttribute>().Select(attribute => attribute.InjectType)
                        .ToImmutableArray();
                    var parameters = constructor.GetParameters().ToImmutableArray();
                    var argumentList = new List<object?>();
                    var argIndex = 0;
                    foreach (var parameter in parameters)
                    {
                        var parameterType = parameter.ParameterType;

                        if (!_componentConstructorCache.TryGetValue(parameterType, out var ctr))
                        {
                            ctr = parameterType.GetConstructor(Array.Empty<Type>());
                            _componentConstructorCache.Add(parameterType, ctr);
                        }

                        if (injectTypes.Contains(parameterType) && ctr != null)
                        {
                            argumentList.Add(ctr.Invoke(null));
                            continue;
                        }

                        if (argIndex < args.Length && args[argIndex].GetType() == parameterType)
                        {
                            argumentList.Add(args[argIndex]);
                            argIndex++;
                            continue;
                        }

                        if (parameter.HasDefaultValue)
                        {
                            argumentList.Add(parameter.DefaultValue!);
                        }
                    }
                    result = (Entity)constructor.Invoke(argumentList.ToArray());
                }
                catch (Exception e)
                {
                    LoggingService.Trace("Error creating entity, constructor invocation threw an exception. Trying other signatures...", e);
                }
            }

            try
            {
                result ??= (Entity?)entityCtr?.Invoke(null);
            }
            catch (Exception e)
            {
                LoggingService.Trace("Error creating entity, default constructor invocation threw an exception.", e);
            }

            if (result == null)
                LoggingService.Error("Could not create entity of type {0}.", entityType.Name);

            return (T?)result;
        }
    }
}
