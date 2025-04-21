using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using PulumiSharp.Reflection;

namespace PulumiSharp;

internal class ValueVisitor<TResult>(object? value) : MemberVisitor<TResult?>
{
    private bool IsObject(Type type)
    {
        return value != null && Convert.GetTypeCode(value) == TypeCode.Object;
    }

    private bool IsList(Type type)
    {
        return value != null && type.IsAssignableTo(typeof(IList));
    }

    private bool IsDictionary(Type type)
    {
        return value != null && type.IsAssignableTo(typeof(IDictionary));
    }

    private static readonly MethodInfo SerializeArrayMethod = typeof(ValueVisitor<TResult>).GetMethod(nameof(SerializeArray), BindingFlags.Static | BindingFlags.NonPublic)!;
    private static readonly MethodInfo SerializeDictionaryMethod = typeof(ValueVisitor<TResult>).GetMethod(nameof(SerializeDictionary), BindingFlags.Static | BindingFlags.NonPublic)!;

    private static ImmutableDictionary<TKey,TValue?> SerializeDictionary<TKey,TValue>(ImmutableDictionary<string,object?> output) where TKey : notnull
    {
        var list = new Dictionary<TKey,TValue?>();

        foreach (var item in output)
        {
            var value = typeof(TValue).Accept(new ValueVisitor<TValue>(item.Value));

            list.Add((TKey)Convert.ChangeType(item.Key,typeof(TKey)),value);
        }

        return list.ToImmutableDictionary();
    }

    private static ImmutableArray<TValue?> SerializeArray<TValue>(ImmutableArray<object> output)
    {
        var list = new List<TValue?>();

        foreach (var item in output)
        {
            var value = typeof(TValue).Accept(new ValueVisitor<TValue>(item));

            list.Add(value);
        }

        return [..list];
    }

    public TResult? CreateList(Type type)
    {
        var array = (ImmutableArray<object?>)value!;

        var castMethod = SerializeArrayMethod.MakeGenericMethod(type.GetGenericArguments());

        var result = castMethod.Invoke(null, [array]);

        return (TResult?)result;
    }

    public TResult? CreateDictionary(Type type)
    {
        var dictionary = (ImmutableDictionary<string, object?>)value!;

        var castMethod = SerializeDictionaryMethod.MakeGenericMethod(type.GetGenericArguments());

        var result = castMethod.Invoke(null, [dictionary]);

        return (TResult?)result;
    }

    public TResult? CreateObject(Type type)
    {
        var arguments = new List<object?>();

        var properties = type.GetProperties();

        var types = properties.Select(c => c.PropertyType).ToArray();

        var dictionary = (ImmutableDictionary<string, object?>)value!;

        foreach (var propertyInfo in properties)
        {
            var propertyValue = dictionary[propertyInfo.Name];

            arguments.Add(propertyInfo.PropertyType.Accept(new ValueVisitor<object>(propertyValue)));
        }

        var constructorInfo = type.GetConstructor(types);

        return (TResult?)constructorInfo?.Invoke(arguments.ToArray())! ?? throw new InvalidOperationException();
    }

    public override TResult? VisitType(Type type)
    {
        if (IsList(type))
        {
            return CreateList(type);
        }

        if (IsDictionary(type))
        {
            return CreateDictionary(type);
        }

        if (IsObject(type))
        {
            return CreateObject(type);
        }
        return CreateValue(type);
    }

    private TResult? CreateValue(Type type)
    {
        if (value == null)
        {
            return default;
        }

        return (TResult?)(value.GetType() == type ? value : Convert.ChangeType(value, type));
    }
}