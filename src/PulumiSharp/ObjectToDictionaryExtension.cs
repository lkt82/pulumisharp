using System.ComponentModel;

namespace PulumiSharp;

internal static class ObjectToDictionaryExtension
{
    public static Dictionary<string, object?> ToDictionary(this object source)
    {
        return source.ToDictionary<object?>();
    }

    public static Dictionary<string, T> ToDictionary<T>(this object? source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source), "Unable to convert object to a dictionary. The source object is null.");
        }

        var dictionary = new Dictionary<string, T>();
        foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
        {
            AddPropertyToDictionary(property, source, dictionary);
        }
        return dictionary;
    }

    private static void AddPropertyToDictionary<T>(PropertyDescriptor property, object source, IDictionary<string, T> dictionary)
    {
        var value = property.GetValue(source);
        if (value != null && IsOfType<T>(value))
        {
            dictionary.Add(property.Name, (T)value);
        }
    }

    private static bool IsOfType<T>(object value)
    {
        return value is T;
    }
}