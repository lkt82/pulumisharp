using System.Reflection;

namespace PulumiSharp.Reflection;

internal static class MemberExtension
{
    public static void Accept(this MemberInfo memberInfo, MemberVisitor visitor)
    {
        switch (memberInfo)
        {
            case Type type:
                type.Accept(visitor);
                break;
            case ConstructorInfo constructor:
                throw new NotSupportedException();
                //constructor.Accept(visitor);
                break;
            case MethodInfo method:
                method.Accept(visitor);
                break;
            case PropertyInfo property:
                property.Accept(visitor);
                break;
        }
    }


    public static IEnumerable<T> GetAllCustomAttributes<T>(this MemberInfo memberInfo) where T : Attribute
    {
        return memberInfo switch
        {
            Type type => type.GetAllCustomAttributes<T>(),
            MethodInfo methodInfo => methodInfo.GetAllCustomAttributes<T>(),
            _ => memberInfo.GetCustomAttributes<T>()
        };
    }

    public static T? GetAllCustomAttribute<T>(this MemberInfo memberInfo) where T : Attribute
    {
        if (memberInfo is Type type)
        {
            return type.GetAllCustomAttribute<T>();
        }

        return memberInfo.GetCustomAttribute<T>();
    }
}