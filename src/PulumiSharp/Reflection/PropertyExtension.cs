using System.Reflection;

namespace PulumiSharp.Reflection;

internal static class PropertyExtension
{
    public static void Accept(this PropertyInfo propertyInfo, MemberVisitor visitor)
    {
        visitor.VisitProperty(propertyInfo);
    }

    public static TResult? Accept<TResult>(this PropertyInfo propertyInfo, MemberVisitor<TResult> visitor)
    {
        return visitor.VisitProperty(propertyInfo);
    }
}
