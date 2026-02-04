using Pulumi.Automation;

namespace PulumiSharp;

public static class StackExtensions
{
    public static PulumiFn ToPulumiFn<TStack>(this TStack stack) where TStack : Stack
    {
        return PulumiFn.Create(() =>
        {
            return OutputSerializer.Serialize(stack.DoBuild());
        });
    }
}
