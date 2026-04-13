using System;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using TailscaleCommandPalette.Helpers;
using TailscaleCommandPalette.Models;

namespace TailscaleCommandPalette.Commands;

internal sealed partial class CopyDnsNameCommand : InvokableCommand
{
    private readonly TailscaleDevice _device;

    public CopyDnsNameCommand(TailscaleDevice device)
    {
        _device = device;
    }

    public override string Name => $"Copy DNS for {_device.HostName}";

    public override CommandResult Invoke()
    {
        try
        {
            ProcessClipboardHelper.SetText(_device.DNSName);
            return CommandResult.Dismiss();
        }
        catch (Exception)
        {
            return CommandResult.KeepOpen();
        }
    }
}
