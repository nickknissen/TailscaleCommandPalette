using System;
using System.Diagnostics;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using TailscaleCommandPalette.Services;

namespace TailscaleCommandPalette.Commands;

internal sealed partial class TailscaleCliCommand : InvokableCommand
{
    private readonly TailscaleCliService _service;
    private readonly string _name;
    private readonly Func<TailscaleCliService, (bool Success, string Message)> _action;

    public TailscaleCliCommand(
        TailscaleCliService service,
        string name,
        Func<TailscaleCliService, (bool Success, string Message)> action)
    {
        _service = service;
        _name = name;
        _action = action;
        Icon = IconHelpers.FromRelativePath("Assets\\Square44x44Logo.scale-200.png");
    }

    public override string Name => _name;

    public override CommandResult Invoke()
    {
        try
        {
            var (success, message) = _action(_service);
            ShowMessage(success ? message : $"Tailscale command failed: {message}", success);
            return success ? CommandResult.Dismiss() : CommandResult.KeepOpen();
        }
        catch (Exception ex)
        {
            ShowMessage($"Unexpected error: {ex.Message}", false);
            return CommandResult.KeepOpen();
        }
    }

    private static void ShowMessage(string message, bool success)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-NoProfile -Command \"Add-Type -AssemblyName PresentationFramework; [System.Windows.MessageBox]::Show('{Escape(message)}', '{(success ? "Tailscale" : "Tailscale Error")}')\"",
            UseShellExecute = false,
            CreateNoWindow = true,
        });
    }

    private static string Escape(string value) => value.Replace("'", "''");
}
