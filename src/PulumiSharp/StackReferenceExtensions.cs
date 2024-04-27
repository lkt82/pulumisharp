using System.Collections.Immutable;
using System.ComponentModel;
using System.Reflection;
using Pulumi;

namespace PulumiSharp;

public static class StackReferenceExtensions
{
    public static Output<T> RequireOutput<T>(this StackReference stackReference, Input<string> name)
    {
        return stackReference.RequireOutput(name).Apply(c => (T)c);
    }

    private static readonly MethodInfo CastMethodInfo = typeof(StackReferenceExtensions).GetMethod(nameof(Cast), BindingFlags.Static | BindingFlags.NonPublic)!;
    private static readonly MethodInfo CastArrayInfo = typeof(StackReferenceExtensions).GetMethod(nameof(CastArray), BindingFlags.Static | BindingFlags.NonPublic)!;
    private static readonly MethodInfo CastDictionaryInfo = typeof(StackReferenceExtensions).GetMethod(nameof(CastDictionary), BindingFlags.Static | BindingFlags.NonPublic)!;

    public static T Get<T>(this StackReference stackReference)
    {
        var arguments = new List<object>();

        foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(typeof(T)))
        {
            var outputType = property.PropertyType.GetGenericArguments().First();

            var castMethod = outputType.IsGenericType switch
            {
                true when outputType.GetGenericTypeDefinition() == typeof(ImmutableArray<>) => CastArrayInfo
                    .MakeGenericMethod(outputType),
                true when outputType.GetGenericTypeDefinition() == typeof(ImmutableDictionary<,>) =>
                    CastDictionaryInfo.MakeGenericMethod(outputType.GetGenericArguments().First(),
                        outputType.GetGenericArguments().Last()),
                _ => CastMethodInfo.MakeGenericMethod(outputType)
            };

            var output = stackReference.GetOutput(property.Name);

            var value = castMethod.Invoke(null, new object[] { output });

            if (value != null)
            {
                arguments.Add(value);
            }
        }

        var types = arguments.Select(c => c.GetType()).ToArray();

        var constructorInfo = typeof(T).GetConstructor(types);

        return (T)constructorInfo?.Invoke(arguments.ToArray())! ?? throw new InvalidOperationException();
    }

    private static Output<T> Cast<T>(Output<object?> output) => output.Apply(c => c is T o ? o : default)!;

    private static Output<ImmutableArray<T>> CastArray<T>(Output<object?> output) => output.Apply(c => ((ImmutableArray<object>)c!).Cast<T>().ToImmutableArray());

    private static Output<ImmutableDictionary<TKey, TValue>> CastDictionary<TKey, TValue>(Output<object?> output) where TKey : notnull => output.Apply(c => ((IEnumerable<KeyValuePair<TKey, object>>)c!).ToImmutableDictionary(keyValuePair => keyValuePair.Key, keyValuePair => (TValue)keyValuePair.Value));
}