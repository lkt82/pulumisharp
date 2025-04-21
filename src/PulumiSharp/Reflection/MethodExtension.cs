using System.Reflection;

namespace PulumiSharp.Reflection;

internal static class MethodExtension
{
    public static void Accept(this MethodInfo methodInfo, MemberVisitor visitor)
    {
        visitor.VisitMethod(methodInfo);
    }

    public static TResult? Accept<TResult>(this MethodInfo methodInfo, MemberVisitor<TResult> visitor)
    {
        return visitor.VisitMethod(methodInfo);
    }

    public static bool IsOverride(this MethodInfo methodInfo)
    {
        return methodInfo.GetBaseDefinition().DeclaringType != methodInfo.DeclaringType;
    }

    public static IEnumerable<T> GetAllCustomAttributes<T>(this MethodInfo methodInfo) where T : Attribute
    {
        var list = new List<T>();

        var current = methodInfo;

        list.AddRange(current.GetCustomAttributes<T>());

        while (current.IsOverride())
        {
            current = methodInfo.GetBaseDefinition();

            list.AddRange(current.GetCustomAttributes<T>());

        }

        list.Reverse();
        return list;
    }
}
