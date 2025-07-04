using System.Reflection;

namespace PulumiSharp.Reflection;

internal static class TypeExtension
{
    public static void Accept(this Type type, MemberVisitor visitor)
    {
        visitor.VisitType(type);
    }

    public static TResult? Accept<TResult>(this Type type, MemberVisitor<TResult> visitor)
    {
        return visitor.VisitType(type);
    }

    public static IEnumerable<MethodInfo> GetAllMethods(this Type type)
    {
        var allMethods = type.GetHierarchy().SelectMany(c => c.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)).Where(c => !c.IsSpecialName).ToList();

        var array = allMethods.ToArray();

        foreach (var methodSymbol in array)
        {
            if (!methodSymbol.IsOverride())
            {
                continue;
            }
            var symbol1 = methodSymbol;
            var index = allMethods.FindIndex(c => c.Name == symbol1.Name);
            allMethods.RemoveAt(index);
        }

        return allMethods;
    }

    public static IEnumerable<Type> GetAllNestedTypes(this Type type)
    {
        return type.GetHierarchy().SelectMany(c => c.GetNestedTypes());
    }

    public static IEnumerable<PropertyInfo> GetAllProperties(this Type type)
    {
        return type.GetHierarchy().SelectMany(c => c.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }

    public static IEnumerable<T> GetAllCustomAttributes<T>(this Type type) where T : Attribute
    {
        return type.GetHierarchy().SelectMany(c => c.GetCustomAttributes<T>(false));
    }

    public static T? GetAllCustomAttribute<T>(this Type type) where T : Attribute
    {
        return type.GetHierarchy().SelectMany(c => c.GetCustomAttributes<T>(false)).FirstOrDefault();
    }

    public static IEnumerable<Type> GetHierarchy(this Type type)
    {
        var types = new List<Type>();

        var currentType = type;

        while (currentType != typeof(object))
        {
            types.Add(currentType);
            if (currentType.BaseType == null)
            {
                break;
            }

            currentType = currentType.BaseType;
        }

        types.AddRange(type.GetInterfaces());

        types.Reverse();

        return types;
    }

}