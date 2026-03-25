[![NuGet](https://img.shields.io/nuget/v/FFmpegVideoPlayer.Avalonia.svg)](https://www.nuget.org/packages/FFmpegVideoPlayer.Avalonia/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

# FFmpegVideoPlayer.Avalonia

Self-contained FFmpeg video player for Avalonia UI.

![Preview](https://raw.githubusercontent.com/jojomondag/FFmpegVideoPlayer.Avalonia/main/images/Preview1.png)

## Installation

### Basic Installation (Video Only)

```bash
dotnet add package FFmpegVideoPlayer.Avalonia
```

The core package works without any additional dependencies. Audio playback is optional.

### FFmpeg Binaries

**By default**, the NuGet package includes FFmpeg binaries for Windows x64 and macOS ARM64. However, you can:

1. **Use your own FFmpeg binaries** (recommended to avoid conflicts):
   ```csharp
   // In Program.cs or before creating VideoPlayerControl
   FFmpegInitializer.Initialize(customPath: @"C:\ffmpeg\bin", useBundledBinaries: false);
   ```

2. **Disable bundled binaries** to reduce package size:
   ```csharp
   FFmpegInitializer.Initialize(useBundledBinaries: false);
   // Will search system PATH and common installation locations
   ```

3. **Use bundled binaries** (default behavior):
   ```csharp
   FFmpegInitializer.Initialize(); // Uses bundled binaries if available
   ```

**Priority order** when initializing:
1. Custom path (if provided) - **highest priority**
2. Bundled binaries (if `useBundledBinaries=true` and present)
3. System discovery (PATH, common installation locations)

### With Audio Support (Optional)

To enable audio playback, also install the OpenTK audio package:

```bash
dotnet add package FFmpegVideoPlayer.Audio.OpenTK
```

Then set the `AudioPlayerFactory` property on `VideoPlayerControl`:

```csharp
using FFmpegVideoPlayer.Audio.OpenTK;
using FFmpegVideoPlayer.Core;

// In your window/control initialization
VideoPlayer.AudioPlayerFactory = (sampleRate, channels) => 
    AudioPlayerFactory.Create(sampleRate, channels);
```

If `AudioPlayerFactory` is not set (or set to null), the player will work in video-only mode without crashing.

### Troubleshooting Audio Issues

If audio is not working, check the following:

1. **OpenAL Native Libraries (Windows)**: OpenTK 4.x does **not** bundle OpenAL native libraries on Windows. You need to install [OpenAL Soft](https://www.openal-soft.org/) separately:
   - Download the Windows installer from [OpenAL Soft Downloads](https://www.openal-soft.org/downloads/)
   - Install it system-wide (places `openal32.dll` in System32)
   - **OR** copy `openal32.dll` to your application's output directory
   - The `AudioPlayerFactory.Create()` method will return `null` if OpenAL is not available, allowing the player to continue in video-only mode

2. **Error Handling**: The `AudioPlayerFactory.Create()` method returns `null` if initialization fails, allowing the player to continue in video-only mode. Check debug output for detailed error messages.

3. **Sample Rate/Format**: Ensure your media file has a supported audio format. The player will automatically resample to stereo S16 format.

4. **Alternative Solutions**: 
   - If you cannot install OpenAL system-wide, you can download `openal32.dll` from OpenAL Soft and place it in your application's output directory (next to your .exe)
   - For distribution, include `openal32.dll` with your application

## Try the Example

```bash
git clone https://github.com/jojomondag/FFmpegVideoPlayer.Avalonia.git
cd FFmpegVideoPlayer.Avalonia/examples/FFmpegVideoPlayerExample
dotnet run
```

## FFmpegMediaPlayer API

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Source` | `string?` | `null` | Video file path or URL |
| `AutoPlay` | `bool` | `false` | Auto-play when media is loaded |
| `Volume` | `int` | `100` | Volume level (0-100) |
| `ShowBuiltInControls` | `bool` | `true` | Show/hide control bar |
| `ShowOpenButton` | `bool` | `true` | Show/hide file picker button |
| `ControlPanelBackground` | `IBrush?` | `White` | Control bar background brush |
| `VideoBackground` | `IBrush?` | `Black` | Video area background (set to `Transparent` for overlays) |
| `VideoStretch` | `Stretch` | `Uniform` | Video stretch mode (`None`, `Fill`, `Uniform`, `UniformToFill`) |
| `EnableKeyboardShortcuts` | `bool` | `true` | Enable keyboard controls (Space, Arrow keys, M) |
| `AudioPlayerFactory` | `Func<int, int, IAudioPlayer?>?` | `null` | Optional factory for audio playback. Set to enable audio, leave null for video-only mode. |
| `IconProvider` | `IIconProvider?` | `null` | Optional custom icon provider. If null, default Avalonia shapes are used. |
| `CurrentMediaPath` | `string?` | `null` | Full path of currently loaded media (read-only) |
| `HasMediaLoaded` | `bool` | `false` | Whether media is currently loaded (read-only) |
| `IsPlaying` | `bool` | `false` | Whether playback is active (read-only) |
| `Position` | `long` | `0` | Current playback position in milliseconds (read-only) |
| `Duration` | `long` | `0` | Total media duration in milliseconds (read-only) |

### Methods

| Method | Parameters | Description |
|--------|------------|-------------|
| `Open(string path)` | `path`: File path or URL | Opens and loads a media file |
| `OpenUri(Uri uri)` | `uri`: Media URI | Opens media from a URI |
| `Play()` | - | Starts or resumes playback |
| `Pause()` | - | Pauses playback |
| `Stop()` | - | Stops playback and resets position |
| `TogglePlayPause()` | - | Toggles between play and pause |
| `Seek(float positionPercent)` | `positionPercent`: 0.0 to 1.0 | Seeks to specific position |
| `ToggleMute()` | - | Toggles mute state |

### Events

| Event | EventArgs | Description |
|-------|-----------|-------------|
| `PlaybackStarted` | `EventArgs` | Raised when playback starts |
| `PlaybackPaused` | `EventArgs` | Raised when playback is paused |
| `PlaybackStopped` | `EventArgs` | Raised when playback is stopped |
| `MediaOpened` | `MediaOpenedEventArgs` | Raised when media is successfully opened |
| `MediaEnded` | `EventArgs` | Raised when media reaches the end |

## Custom Icons

The player uses standard Avalonia shapes for icons by default (no Material.Icons dependency). To use custom icons, implement `IIconProvider`:

```csharp
public class MyIconProvider : IIconProvider
{
    public Geometry CreatePlayIcon() => /* your custom geometry */;
    public Geometry CreatePauseIcon() => /* your custom geometry */;
    // ... implement other methods
}

// Then set it on the control
VideoPlayer.IconProvider = new MyIconProvider();
```

## Dependencies

### Required
- **Avalonia** (11.3.6+) - UI framework
- **FFmpegVideoPlayer.Core** - Core player logic (included)

### Optional
- **FFmpegVideoPlayer.Audio.OpenTK** - Audio playback support (optional, video-only mode works without it)
- **Material.Icons.Avalonia** - Not required. Icons use standard Avalonia shapes by default.

## Platform Support

| Platform | Bundled Binaries | Custom Binaries |
|----------|------------------|-----------------|
| Windows x64 | ✅ Optional | ✅ Supported |
| macOS ARM64 | ✅ Optional | ✅ Supported |
| macOS x64 | ⚠️ Not bundled | ✅ Supported (use custom path) |
| Linux | ⚠️ Not bundled | ✅ Supported (use custom path) |

**Note**: Bundled binaries are optional. You can use your own FFmpeg installation by providing a `customPath` to `FFmpegInitializer.Initialize()`. This avoids conflicts with other libraries and reduces package size.

## License

MIT
