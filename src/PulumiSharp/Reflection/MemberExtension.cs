using System.Reflection;

namespace PulumiSharp.Reflection;

internal static class MemberExtension
{
    public static void Accept(this MemberInfo memberInfo, MemberVisitor visitor)
    {
        switch (memberInfo)
        {
            case Type type:
                TypeExtension.Accept(type, visitor);
                break;
            case ConstructorInfo constructor:
                constructor.Accept(visitor);
                break;
            case MethodInfo method:
                MethodExtension.Accept(method, visitor);
                break;
            case PropertyInfo property:
                PropertyExtension.Accept(property, visitor);
                break;
        }
    }


    public static IEnumerable<T> GetAllCustomAttributes<T>(this MemberInfo memberInfo) where T : Attribute
    {
        if (memberInfo is Type type)
        {
            return TypeExtension.GetAllCustomAttributes<T>(type);
        }


        if (memberInfo is MethodInfo methodInfo)
        {
            return MethodExtension.GetAllCustomAttributes<T>(methodInfo);
        }

        return memberInfo.GetCustomAttributes<T>();
    }

    public static T? GetAllCustomAttribute<T>(this MemberInfo memberInfo) where T : Attribute
    {
        if (memberInfo is Type type)
        {
            return TypeExtension.GetAllCustomAttribute<T>(type);
        }

        return memberInfo.GetCustomAttribute<T>();
    }
}