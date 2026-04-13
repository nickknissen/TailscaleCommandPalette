using System;
using System.Diagnostics;
using System.Text;

namespace TailscaleCommandPalette.Helpers;

internal static class ProcessClipboardHelper
{
    public static void SetText(string text)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "clip.exe",
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardInputEncoding = Encoding.UTF8,
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start clip.exe.");

        process.StandardInput.Write(text ?? string.Empty);
        process.StandardInput.Close();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"clip.exe exited with code {process.ExitCode}.");
        }
    }
}
