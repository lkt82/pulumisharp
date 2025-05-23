using System.Collections;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Reflection;
using Pulumi;
using PulumiSharp.Reflection;

namespace PulumiSharp;

internal class OutputVisitor<T>(Output<ImmutableDictionary<string, object>> output) : MemberVisitor<T> where T : class
{
    private static readonly MethodInfo UnboxMethodInfo = typeof(OutputVisitor<T>).GetMethod(nameof(UnboxOutput), BindingFlags.Static | BindingFlags.NonPublic)!;
    private static Output<TValue?> UnboxOutput<TValue>(Output<object?> output) => output.Apply(c =>
    {
        var value = typeof(TValue).Accept(new ValueVisitor<TValue>(c));
        return value ?? default;
    });

    private readonly NullabilityInfoContext _nullabilityContext = new();

    public override T VisitType(Type type)
    {
        var arguments = new List<object?>();

        foreach (var propertyInfo in type.GetAllProperties())
        {
            if (!IsOutput(propertyInfo))
            {
                throw new NotSupportedException($"property {propertyInfo.Name} is not a output type");
            }

            var value = MapProperty(propertyInfo);

            arguments.Add(value);
        }

        var types = arguments.Where(c => c != null).Select(c => c!.GetType()).ToArray();
        var constructorInfo = typeof(T).GetConstructor(types);

        return (T)constructorInfo?.Invoke(arguments.ToArray())! ?? throw new InvalidOperationException("constructor not found");
    }

    private object? MapProperty(PropertyInfo propertyInfo)
    {
        var castMethod = UnboxMethodInfo.MakeGenericMethod(propertyInfo.PropertyType.GetGenericArguments());

        var nullabilityInfo = _nullabilityContext.Create(propertyInfo);

        if (nullabilityInfo.WriteState is NullabilityState.Nullable)
        {
            var propertyOutput = output.Apply(c => c.GetValueOrDefault(propertyInfo.Name));

            return castMethod.Invoke(null, [propertyOutput]);
        }
        else
        {
            var propertyOutput = output.Apply(c =>
            {
                if (!c.TryGetValue(propertyInfo.Name, out var expression))
                {
                    throw new InvalidOperationException($"output {propertyInfo.Name} is missing");
                }

                return expression;
            });

            return castMethod.Invoke(null, [propertyOutput]);
        }
    }

    private static bool IsOutput(PropertyInfo propertyInfo)
    {
        return propertyInfo.PropertyType.IsGenericType & propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Output<>);
    }
}

internal class OutputVisitor(object? output) : MemberVisitor<IDictionary<string, object?>>
{
    private static readonly MethodInfo UnboxMethodInfo = typeof(OutputVisitor).GetMethod(nameof(UnboxOutput), BindingFlags.Static | BindingFlags.NonPublic)!;

    private static Output<object>? UnboxOutput<T>(Type type, object? output)
    {
        var typeOutput = (Output<T>?)output;

        if (typeOutput == null)
        {
            return null;
        }

        return typeOutput.Apply(c => SerializeObject(type, c))!;
    }

    private static object? SerializeObject(Type type, object? value)
    {
        if (IsOutput(type, value))
        {
            var outputType = type.GetGenericArguments().First();

            var unboxMethod = UnboxMethodInfo.MakeGenericMethod(outputType);

            return unboxMethod.Invoke(null, [outputType, value]);
        }

        if (IsDictionary(type, value))
        {
            var dictionary = new Dictionary<string, object?>();
            foreach (var key in ((IDictionary)value!).Keys)
            {
                var keyValue = ((IDictionary)value)[key];

                var newValue = keyValue != null ? SerializeObject(keyValue.GetType(), keyValue) : null;

                dictionary.Add(Convert.ToString(key)!, newValue);
            }
            return dictionary.ToImmutableDictionary();
        }

        if (IsList(type, value))
        {
            var list = new List<object?>();
            foreach (var listItem in (IList)value!)
            {
                var newValue = listItem != null ? SerializeObject(listItem.GetType(), listItem) : null;

                list.Add(newValue);
            }

            return list.ToImmutableArray();
        }

        if (IsObject(type, value))
        {
            var dictionary = new Dictionary<string, object?>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value!))
            {
                var propertyValue = property.GetValue(value);

                var newValue = propertyValue != null ? SerializeObject(property.PropertyType, propertyValue) : null;

                dictionary.Add(property.Name, newValue);
            }

            return dictionary.ToImmutableDictionary();
        }

        return value;
    }

    private static bool IsObject(Type type, object? value)
    {
        return value != null && Convert.GetTypeCode(value) == TypeCode.Object;
    }

    private static bool IsList(Type type, object? value)
    {
        return value != null && type.IsAssignableTo(typeof(IList));
    }

    private static bool IsDictionary(Type type, object? value)
    {
        return value != null && type.IsAssignableTo(typeof(IDictionary));
    }

    private static bool IsOutput(Type type, object? value)
    {
        return value != null && type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableTo(typeof(Output<>));
    }

    public override IDictionary<string, object?> VisitType(Type type)
    {
        if (output == null)
        {
            return new Dictionary<string, object?>();
        }

        if (Convert.GetTypeCode(output) != TypeCode.Object)
        {
            throw new NotSupportedException($"output of type {type.Name} is not supported. please use a object or record");
        }

        var serialized = (IDictionary<string, object?>?)SerializeObject(type, output);

        return serialized ?? ImmutableDictionary<string, object?>.Empty;
    }
}