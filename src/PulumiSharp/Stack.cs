namespace PulumiSharp;

public abstract class Stack
{
    public Dictionary<string, object?> Outputs { get; protected set; } = new();
}

public abstract class Stack<T> : Stack where T : class
{
    protected void RegisterOutputs(T output)
    {
        Outputs= output.ToDictionary();
    }
}