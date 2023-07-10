using System.Reflection;
using System.Runtime.ExceptionServices;
using Pulumi.Automation;

namespace PulumiSharp;

internal class InlineLanguageHost : IAsyncDisposable
{
    private readonly IAsyncDisposable _inner;
    private static readonly Type LanguageHostType = typeof(InlineProgramArgs).Assembly.GetType("Pulumi.Automation.WorkspaceStack+InlineLanguageHost")!;

    internal InlineLanguageHost(IAsyncDisposable inner,int port)
    {
        _inner = inner;
        Port = port;
    }

    public int Port { get; set; }

    public static async Task<InlineLanguageHost> Start(LocalWorkspaceOptions options)
    {
        var inlineHost = (IAsyncDisposable)Activator.CreateInstance(LanguageHostType, options.Program!, options.Logger, CancellationToken.None)!;

        var startAsync = LanguageHostType.GetMethod("StartAsync", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        await((Task)startAsync!.Invoke(inlineHost, null)!).ConfigureAwait(false);

        var getPortAsync = LanguageHostType.GetMethod("GetPortAsync", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        var port = await((Task<int>)getPortAsync!.Invoke(inlineHost, null)!).ConfigureAwait(false);

        return new InlineLanguageHost(inlineHost, port);
    }

    public bool TryGetExceptionInfo(out ExceptionDispatchInfo exception)
    {
        var method = LanguageHostType.GetMethod("TryGetExceptionInfo", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        var inputParams = new ExceptionDispatchInfo[] { null! };
        // ReSharper disable once CoVariantArrayConversion
        var result = (bool)method!.Invoke(_inner, inputParams)!;

        exception = inputParams[0];
        return result;
    }

    public ValueTask DisposeAsync()
    {
        return _inner.DisposeAsync();
    }
}