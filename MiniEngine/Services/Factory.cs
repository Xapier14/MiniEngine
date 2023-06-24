using MiniEngine.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace MiniEngine
{
    public static class Factory
    {
        private static readonly Dictionary<Type, ConstructorInfo[]> _entityInjectedConstructorCache = new();
        private static readonly Dictionary<Type, ConstructorInfo?> _entityDefaultConstructorCache = new();
        private static readonly Dictionary<Type, ConstructorInfo?> _componentConstructorCache = new();

        public static T? TryCreateEntity<T>(params object[] args) where T : Entity
            => (T?)TryCreateEntity(typeof(T), args);

        public static Entity? TryCreateEntity(Type type, params object[] args)
        {
            var entityType = type;
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

            return result;
        }
        public static Scene BuildScene(MemoryResource xmlMemoryResource)
        {
            using var stream = xmlMemoryResource.CreateStream();
            return BuildScene(stream);
        }

        public static Scene BuildSceneXml(string xmlFilePath)
        {
            using var stream = File.OpenRead(xmlFilePath);
            return BuildScene(stream);
        }

        private static Scene BuildScene(Stream stream)
        {
            var scene = new Scene();
            var typeResolver = new TypeResolver();

            // base references
            typeResolver.AddReference("MiniEngine");
            typeResolver.AddReference("MiniEngine.Components");
            var executingAssembly = Assembly.GetEntryAssembly()?.GetName().Name;
            if (executingAssembly != null)
                typeResolver.AddReference(executingAssembly);

            // parse xml stream
            var document = new XmlDocument();
            document.Load(stream);
            var rootElement = document["Scene"]!;
            var references = rootElement["References"];
            var properties = rootElement["Properties"];
            var entities = rootElement["Entities"];
            try
            {
                if (references != null)
                {
                    foreach (XmlElement reference in references.ChildNodes)
                    {
                        typeResolver.AddReference(reference.InnerText);
                    }
                }

                if (properties != null)
                {
                    if (properties["ViewPosition"] is { } viewPosition)
                    {
                        var x = int.Parse(viewPosition.Attributes["X"]?.Value ?? "0");
                        var y = int.Parse(viewPosition.Attributes["Y"]?.Value ?? "0");
                        scene.ViewPosition.Set(x, y);
                    }
                    // add other properties
                }

                if (entities != null)
                {
                    foreach (XmlElement entityNode in entities.ChildNodes)
                    {
                        var entityTypeName = entityNode.Name;
                        var entityType = typeResolver.ResolveType(entityTypeName);
                        if (entityType == null)
                            throw new EntityCreationFailException($"Could not resolve type of {entityTypeName}.");
                        var entity = TryCreateEntity(entityType);
                        if (entity == null)
                            throw new EntityCreationFailException($"Failed creating instance of entity {entityTypeName}.");

                        // configure entity specific component configuration
                        foreach (XmlElement componentNode in entityNode.ChildNodes)
                        {
                            var componentName = componentNode.Name;
                            var componentType = typeResolver.ResolveType(componentName);
                            if (componentType == null)
                            {
                                LoggingService.Warn("Warning, failed to resolve type of {0}.", componentName);
                                continue;
                            }
                            var component = entity.GetComponent(componentType);
                            if (component == null)
                            {
                                LoggingService.Warn("Warning, entity {0} does not have component {1}.", entityTypeName, componentName);
                                continue;
                            }
                            // attribute parser
                            foreach (XmlAttribute attribute in componentNode.Attributes)
                            {
                                var attributeName = attribute.Name;
                                var member = componentType.GetMember(attributeName)
                                .FirstOrDefault(memberInfo =>
                                    memberInfo.IsPublic() &&
                                    (memberInfo.MemberType.HasFlag(MemberTypes.Property) ||
                                    memberInfo.MemberType.HasFlag(MemberTypes.Field)));
                                if (member == null)
                                {
                                    LoggingService.Warn("Warning, attribute {0} of component {1} in entity {2} does not exist as a public property of field.",
                                        attributeName,
                                        componentName,
                                        entityTypeName);
                                    continue;
                                }
                                var value = TypeResolver.AutoParse(attribute.Value, member.GetUnderlyingType()!);
                                if (member.MemberType.HasFlag(MemberTypes.Field))
                                {
                                    var fieldInfo = (FieldInfo)member;
                                    fieldInfo.SetValue(component, value);
                                }
                                else if (member.MemberType.HasFlag(MemberTypes.Property))
                                {
                                    var propertyInfo = (PropertyInfo)member;
                                    propertyInfo.SetValue(component, value);
                                }
                            }
                            // child node parser
                            foreach (XmlElement componentPropertyNode in componentNode.ChildNodes)
                            {
                                var componentPropertyName = componentPropertyNode.Name;
                                var member = componentType.GetMember(componentPropertyName)
                                    .FirstOrDefault(memberInfo =>
                                        memberInfo.IsPublic() &&
                                        (memberInfo.MemberType.HasFlag(MemberTypes.Property) ||
                                         memberInfo.MemberType.HasFlag(MemberTypes.Field)));
                                if (member == null)
                                {
                                    LoggingService.Warn("Warning, child element {0} of component {1} in entity {2} does not exist as a public property of field.",
                                        componentPropertyName,
                                        componentName,
                                        entityTypeName);
                                    continue;
                                }

                                var value = componentPropertyNode.Value != null
                                    ? TypeResolver.AutoParse(componentPropertyNode.Value, member.GetUnderlyingType()!)
                                    : null;
                                if (value == null)
                                {
                                    // complex type
                                    object? memberInstance = null;
                                    if (member.MemberType.HasFlag(MemberTypes.Field))
                                    {
                                        var fieldInfo = (FieldInfo)member;
                                        memberInstance = fieldInfo.GetValue(component);
                                    }
                                    else if (member.MemberType.HasFlag(MemberTypes.Property))
                                    {
                                        var propertyInfo = (PropertyInfo)member;
                                        memberInstance = propertyInfo.GetValue(component);
                                    }

                                    if (memberInstance == null)
                                    {
                                        LoggingService.Warn("Warning, could not get value handle of child element {0} of component {1} in entity {2}.",
                                            componentPropertyName,
                                            componentName,
                                            entityTypeName);
                                        continue;
                                    }

                                    foreach (XmlAttribute attribute in componentPropertyNode.Attributes)
                                    {
                                        var attributeName = attribute.Name;
                                        var subMember = memberInstance.GetType().GetMember(attributeName)
                                            .FirstOrDefault(memberInfo =>
                                                memberInfo.IsPublic() &&
                                                (memberInfo.MemberType.HasFlag(MemberTypes.Property) ||
                                                 memberInfo.MemberType.HasFlag(MemberTypes.Field)));
                                        if (subMember == null)
                                        {
                                            LoggingService.Warn("Warning, attribute {0} of sub-component {1} of component {2} in entity {3} does not exist as a public property of field.",
                                                attributeName,
                                                componentPropertyName,
                                                componentName,
                                                entityTypeName);
                                            continue;
                                        }
                                        var subValue = TypeResolver.AutoParse(attribute.Value, subMember.GetUnderlyingType()!);
                                        if (subMember.MemberType.HasFlag(MemberTypes.Field))
                                        {
                                            var fieldInfo = (FieldInfo)subMember;
                                            fieldInfo.SetValue(memberInstance, subValue);
                                        }
                                        else if (subMember.MemberType.HasFlag(MemberTypes.Property))
                                        {
                                            var propertyInfo = (PropertyInfo)subMember;
                                            propertyInfo.SetValue(memberInstance, subValue);
                                        }
                                    }

                                    value = memberInstance;
                                }
                                // simple type
                                if (member.MemberType.HasFlag(MemberTypes.Field))
                                {
                                    var fieldInfo = (FieldInfo)member;
                                    fieldInfo.SetValue(component, value);
                                }
                                else if (member.MemberType.HasFlag(MemberTypes.Property))
                                {
                                    var propertyInfo = (PropertyInfo)member;
                                    propertyInfo.SetValue(component, value);
                                }
                            }
                        }

                        scene.AddEntity(entity);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SceneBuildFailException("Failed to build scene.", ex);
            }

            return scene;
        }
    }
}
