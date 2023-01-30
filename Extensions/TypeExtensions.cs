using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniEngine
{
    public static class TypeExtensions
    {
        public static bool IsOfGenericType(this Type thisType, Type genericType)
        {
            var type = thisType;
            while (type is not null)
            {
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == genericType)
                    return true;
                type = type.BaseType;
            }

            return false;
        }
    }
}
