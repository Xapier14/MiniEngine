using System;
using System.Reflection;

namespace MiniEngine
{
    public static class MemberInfoExtensions
    {
        public static Type? GetUnderlyingType(this MemberInfo memberInfo)
        {
            return memberInfo.MemberType switch
            {
                MemberTypes.Event => ((EventInfo)memberInfo).EventHandlerType,
                MemberTypes.Field => ((FieldInfo)memberInfo).FieldType,
                MemberTypes.Method => ((MethodInfo)memberInfo).ReturnType,
                MemberTypes.Property => ((PropertyInfo)memberInfo).PropertyType,
                _ => null
            };
        }
        public static bool IsPublic(this MemberInfo memberInfo)
        {
            return memberInfo.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)memberInfo).IsPublic,
                MemberTypes.Method => ((MethodInfo)memberInfo).IsPublic,
                MemberTypes.Property => ((PropertyInfo)memberInfo).SetMethod?.IsPublic() ?? false,
                _ => false
            };
        }
    }
}
