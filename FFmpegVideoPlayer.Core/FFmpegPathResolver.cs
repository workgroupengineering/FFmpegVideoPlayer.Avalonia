using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace FFmpegVideoPlayer.Core;

/// <summary>
/// Locates packaged FFmpeg binaries (NuGet runtimes) and configures the native search path.
/// </summary>
internal static class FFmpegPathResolver
{
    /// <summary>
    /// Tries to find a bundled FFmpeg path under runtimes/&lt;rid&gt;/native and, if found,
    /// configures the process search path so the native loader can locate dependencies.
    /// </summary>
    public static string? TryConfigureBundledFFmpeg()
    {
        var path = FindBundledPath();
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        ConfigureNativeSearchPath(path);
        return path;
    }

    /// <summary>
    /// Returns runtimes/&lt;rid&gt;/native if it contains FFmpeg binaries.
    /// </summary>
    public static string? FindBundledPath()
    {
        var baseDir = AppContext.BaseDirectory;
        var rid = GetRuntimeIdentifier();

        var candidates = new List<string>
        {
            Path.Combine(baseDir, "runtimes", rid, "native"),
            Path.Combine(baseDir, rid),
            Path.Combine(baseDir, "native", rid),
            Path.Combine(baseDir, "ffmpeg", rid),
            Path.Combine(baseDir, "ffmpeg", "bin")
        };

        // Fallback for universal macOS builds if provided as osx-universal/native
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            candidates.Add(Path.Combine(baseDir, "runtimes", "osx-universal", "native"));
        }

        foreach (var candidate in candidates)
        {
            if (HasFFmpegLibrary(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    /// <summary>
    /// Ensures the native loader can locate FFmpeg binaries from a specific folder.
    /// </summary>
    public static void ConfigureNativeSearchPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            return;

        // Hint FFmpeg.AutoGen to load from this folder
        ffmpeg.RootPath = path;

        AddPathVariable("PATH", path);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            AddPathVariable("DYLD_LIBRARY_PATH", path);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            AddPathVariable("LD_LIBRARY_PATH", path);
        }

        // Initialize the dynamically loaded bindings after setting the path
        InitializeBindings();
    }

    /// <summary>
    /// Initializes (or re-initializes) the FFmpeg.AutoGen dynamic bindings.
    /// Must be called after setting <see cref="ffmpeg.RootPath"/>. Safe to call multiple
    /// times — each call re-resolves every function against the current RootPath.
    /// </summary>
    public static void InitializeBindings()
    {
        // FFmpeg.AutoGen 8.x's DynamicallyLoadedBindings.Initialize is idempotent and
        // re-probes every function, so we can call it again after the search path changes.
        DynamicallyLoadedBindings.Initialize();
#if DEBUG
        Console.WriteLine($"[FFmpegPathResolver] Bindings initialized (RootPath: {ffmpeg.RootPath})");
#endif
    }

    /// <summary>
    /// Probes whether the currently-configured FFmpeg libraries actually loaded and
    /// exported their functions. FFmpeg.AutoGen silently swallows dlopen/LoadLibrary
    /// failures and replaces unresolved functions with stubs that throw
    /// NotSupportedException at call time — so "bindings initialized" does not imply
    /// "native library loaded". Calling a trivial function and checking for a non-zero
    /// return is the cheapest reliable check.
    /// </summary>
    public static bool TryValidateBindings()
    {
        try
        {
            return ffmpeg.avcodec_version() != 0;
        }
        catch
        {
            return false;
        }
    }

    public static string GetRuntimeIdentifier()
    {
        var os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" :
                 RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" :
                 RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux" : "unknown";

        var arch = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            Architecture.Arm => "arm",
            _ => "x64"
        };

        return $"{os}-{arch}";
    }

    public static bool HasFFmpegLibrary(string path)
    {
        if (!Directory.Exists(path))
            return false;

        var libraryPatterns = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new[] { "avcodec*.dll", "avformat*.dll", "avutil*.dll" }
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? new[] { "libavcodec*.dylib", "libavformat*.dylib", "libavutil*.dylib" }
                : new[] { "libavcodec.so*", "libavformat.so*", "libavutil.so*" };

        foreach (var pattern in libraryPatterns)
        {
            try
            {
                if (Directory.GetFiles(path, pattern).Length > 0)
                    return true;
            }
            catch
            {
                // Ignore IO issues and continue checking other patterns
            }
        }

        return false;
    }

    private static void AddPathVariable(string variable, string path)
    {
        var current = Environment.GetEnvironmentVariable(variable) ?? string.Empty;
        var parts = current.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Any(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase)))
            return;

        var newValue = string.IsNullOrEmpty(current)
            ? path
            : $"{path}{Path.PathSeparator}{current}";

        Environment.SetEnvironmentVariable(variable, newValue);
    }
}

