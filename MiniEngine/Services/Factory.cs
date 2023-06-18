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
    public static class Factory
    {
        private static readonly Dictionary<Type, ConstructorInfo[]> _entityInjectedConstructorCache = new();
        private static readonly Dictionary<Type, ConstructorInfo?> _entityDefaultConstructorCache = new();
        private static readonly Dictionary<Type, ConstructorInfo?> _componentConstructorCache = new();

        public static T? TryCreateEntity<T>(params object[] args) where T : Entity
        {
            var entityType = typeof(T);
            if (entityType.IsAssignableFrom(typeof(Entity)))
            {
                LoggingService.Error("Error creating entity {0}, type is not assignable to Entity.", entityType.Name);
                return null;
            }

            if (!_entityInjectedConstructorCache.TryGetValue(entityType, out var constructors))
            {
                constructors = entityType.GetConstructors();
                _entityInjectedConstructorCache.Add(entityType, constructors);
            }
            if (!_entityDefaultConstructorCache.TryGetValue(entityType, out var entityCtr))
            {
                entityCtr = entityType.GetConstructor(Array.Empty<Type>());
                _entityDefaultConstructorCache.Add(entityType, entityCtr);
            }
            
            var autoInjectTypes = ImmutableArray<Type>.Empty;
            Entity? result = null;
            foreach (var constructor in constructors)
            {
                try
                {
                    var autoInjectAttributes = constructor.GetCustomAttributes()
                        .Where(attribute => attribute.GetType().IsAssignableTo(typeof(IAutoInjectAttribute)));
                    autoInjectTypes = autoInjectAttributes.Cast<IAutoInjectAttribute>().Select(attribute => attribute.InjectType)
                        .ToImmutableArray();

                    var injectAttributes = constructor.GetCustomAttributes()
                        .Where(attribute => attribute.GetType().IsAssignableTo(typeof(IInjectAttribute)));
                    var injectTypes = injectAttributes.Cast<IInjectAttribute>().Select(attribute => attribute.InjectType)
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

                        var isValidInjectType = injectTypes.Contains(parameterType) ||
                                                injectTypes.Any(type => type.IsAssignableFrom(parameterType));

                        if (isValidInjectType && ctr != null)
                        {
                            argumentList.Add(ctr.Invoke(null));
                            continue;
                        }

                        if (argIndex < args.Length && args[argIndex].GetType().IsAssignableTo(parameterType))
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

                    if (parameters.Length != argumentList.Count)
                        continue;
                    result = (Entity)constructor.Invoke(argumentList.ToArray());

                    var components = argumentList.Where(argument =>
                        argument?.GetType().IsAssignableTo(typeof(Component)) == true).Cast<Component>();
                    foreach (var component in components)
                        result.AddComponent(component);

                    break;
                }
                catch (Exception e)
                {
                    LoggingService.Trace("Error creating entity, constructor invocation threw an exception. Trying other signatures...", e);
                }
            }

            if (result == null && entityCtr == null)
            {
                LoggingService.Error("Could not create entity of type {0}. No suitable constructor found.", entityType.Name);
                return null;
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
            {
                LoggingService.Error("Could not create entity of type {0}.", entityType.Name);
                return null;
            }


            // inject auto/anonymous components
            foreach (var autoInjectType in autoInjectTypes)
            {
                if (!_componentConstructorCache.TryGetValue(autoInjectType, out var ctr))
                {
                    ctr = autoInjectType.GetConstructor(Array.Empty<Type>());
                    _componentConstructorCache.Add(autoInjectType, ctr);
                }
                var component = (Component?)ctr?.Invoke(null);
                if (component is null)
                {
                    LoggingService.Error("Entity requires component {0}, but component construction failed.",
                        autoInjectType.Name);
                    continue;
                }
                result.AddComponent(component);
            }

            return (T)result;
        }
    }
}
