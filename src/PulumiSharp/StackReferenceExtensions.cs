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


    public static T Get<T>(this StackReference stackReference)
    {
        var arguments = new List<object>();

        foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(typeof(T)))
        {
            var outputType = property.PropertyType.GetGenericArguments().First();

            MethodInfo castMethod;

            if (outputType.IsGenericType && outputType.GetGenericTypeDefinition() == typeof(ImmutableArray<>))
            { 
                castMethod = CastArrayInfo.MakeGenericMethod(outputType.GetGenericArguments().First());
            }
            else
            { 
                castMethod = CastMethodInfo.MakeGenericMethod(outputType);
            }

            var output = stackReference.GetOutput(property.Name);

            var value = castMethod.Invoke(null,new object[]{output});

            if (value != null)
            {
                arguments.Add(value);
            }
        }

        var types = arguments.Select(c => c.GetType()).ToArray();

        var constructorInfo = typeof(T).GetConstructor(types);

        return (T)constructorInfo?.Invoke(arguments.ToArray())! ?? throw new InvalidOperationException();
    }

    private static Output<T> Cast<T>(Output<object?> output)
    {
        return output.Apply(c => (T)c!);
    }

    private static Output<ImmutableArray<T>> CastArray<T>(Output<object?> output)
    {
        return output.Apply(c => ((ImmutableArray<object>)c!).Cast<T>().ToImmutableArray());
    }
}