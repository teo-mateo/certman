using System.Diagnostics;

namespace certman.Services;

public static class ProcessExtensions
{
    public static void ThrowIfBadExit(this Process process, string? error)
    {
        if (process.HasExited && process.ExitCode != 0)
            throw new Exception($"OpenSSL exited with code {process.ExitCode}. Error message: {error ?? "none"}");
    }
}