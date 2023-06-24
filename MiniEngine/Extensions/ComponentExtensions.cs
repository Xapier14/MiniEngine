using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MiniEngine
{
    public static class ComponentExtensions
    {
        public static IEnumerable<Type> GetRequiredComponents(this Component component)
        {
            return component.GetType().GetCustomAttributes()
                .Where(attribute => attribute.GetType()
                    .IsOfGenericType(typeof(RequiresComponentAttribute<>)))
                .Select(attribute => ((IRequiresComponentAttribute)attribute).RequiredComponentType);
        }
    }
}
