using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CommandPalette.Extensions;

namespace TailscaleCommandPalet;

[Guid("d4e5f6a7-b8c9-4d0e-a1b2-c3d4e5f6a7b8")]
public sealed partial class TailscaleCommandPalet : IExtension, IDisposable
{
    private readonly ManualResetEvent _extensionDisposedEvent;

    private readonly TailscaleCommandPaletCommandsProvider _provider = new();

    public TailscaleCommandPalet(ManualResetEvent extensionDisposedEvent)
    {
        this._extensionDisposedEvent = extensionDisposedEvent;
    }

    public object? GetProvider(ProviderType providerType)
    {
        return providerType switch
        {
            ProviderType.Commands => _provider,
            _ => null,
        };
    }

    public void Dispose() => this._extensionDisposedEvent.Set();
}
