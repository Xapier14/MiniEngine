using MiniEngine.Utility;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace MiniEngine
{
    internal static class SceneBuilder
    {
        public static Scene BuildScene(Stream stream)
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

                if (entities != null)
                {
                    foreach (XmlElement entityNode in entities.ChildNodes)
                    {
                        var entityTypeName = entityNode.Name;
                        var entityType = typeResolver.ResolveType(entityTypeName);
                        if (entityType == null)
                            throw new EntityCreationFailException($"Could not resolve type of {entityTypeName}.");
                        var entity = EntityFactory.TryCreateEntity(entityType);
                        if (entity == null)
                            throw new EntityCreationFailException($"Failed creating instance of entity {entityTypeName}.");

                        // parse entity attributes
                        foreach (XmlAttribute attribute in entityNode.Attributes)
                        {
                            var attributeName = attribute.Name;
                            var member = entityType.GetMember(attributeName)
                                .FirstOrDefault(memberInfo =>
                                    memberInfo.IsPublic() &&
                                    (memberInfo.MemberType.HasFlag(MemberTypes.Property) ||
                                     memberInfo.MemberType.HasFlag(MemberTypes.Field)));
                            if (member == null)
                            {
                                LoggingService.Warn("Warning, attribute {0} of entity {1} does not exist as a public property of field.",
                                    attributeName,
                                    entityTypeName);
                                continue;
                            }

                            var memberType = member.GetUnderlyingType()!;
                            var genericTypeArgs = memberType.GetGenericArguments();
                            if (memberType.IsGenericType)
                                memberType = memberType.GetGenericTypeDefinition();
                            if (memberType == typeof(Nullable<>))
                                memberType = genericTypeArgs.First();
                            var value = TypeResolver.AutoParse(attribute.Value, memberType);
                            if (member.MemberType.HasFlag(MemberTypes.Field))
                            {
                                var fieldInfo = (FieldInfo)member;
                                fieldInfo.SetValue(entity, value);
                            }
                            else if (member.MemberType.HasFlag(MemberTypes.Property))
                            {
                                var propertyInfo = (PropertyInfo)member;
                                propertyInfo.SetValue(entity, value);
                            }
                        }

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
                            var component = entity.TryGetComponent(componentType);
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

                if (properties != null)
                {
                    if (properties["ViewPosition"] is { } viewPosition)
                    {
                        var x = int.Parse(viewPosition.Attributes["X"]?.Value ?? "0");
                        var y = int.Parse(viewPosition.Attributes["Y"]?.Value ?? "0");
                        scene.ViewPosition.Set(x, y);
                    }
                    if (properties["ViewRotation"] is { } viewRotation)
                    {
                        var rotationString = viewRotation.Attributes.GetNamedItem("Value")?.Value ?? viewRotation.Value ?? "0.0";
                        scene.ViewRotation = float.Parse(rotationString);
                    }

                    if (properties["FollowEntity"] is { } followEntity)
                    {
                        var followEntityName = followEntity.Attributes.GetNamedItem("Name")?.Value ?? followEntity.Value;
                        if (followEntityName != null)
                        {
                            scene.FollowEntity = scene.GetEntity(followEntityName);
                        }
                    }

                    if (properties["BackgroundColor"] is { } backgroundColor)
                    {
                        var colorString = backgroundColor.Attributes.GetNamedItem("Value")?.Value ?? backgroundColor.Value ?? "0.0";
                        scene.BackgroundColor = Color.Parse(colorString);
                    }
                    // add other properties
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
