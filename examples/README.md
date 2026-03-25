# FFmpegVideoPlayer.Avalonia Examples

This folder contains example projects demonstrating how to use the FFmpegVideoPlayer.Avalonia library.

## FFmpegVideoPlayerExample

A simple example showing the basic usage of the `VideoPlayerControl`.

### Running the Example

```bash
cd examples/FFmpegVideoPlayerExample
dotnet run
```

### What it demonstrates

- Basic setup with `FFmpegInitializer.Initialize()`
- Using `VideoPlayerControl` in XAML
- Optional audio support via `AudioPlayerFactory` property (requires FFmpegVideoPlayer.Audio.OpenTK package)
  - **Note**: On Windows, OpenAL Soft must be installed or `openal32.dll` must be present. See main README for details.
- Default icons using standard Avalonia shapes (no Material.Icons dependency)
- FFmpeg initialization options (see `Program.cs` for examples of custom path usage)

### Project Structure

- `Program.cs` - Entry point with FFmpeg initialization
- `App.axaml` - Application setup with required styles
- `MainWindow.axaml` - Main window with VideoPlayerControl

## Using the NuGet Package Instead

If you want to test with the published NuGet package instead of the local project reference, modify the `.csproj`:

```xml
<!-- Remove this -->
<ProjectReference Include="../../Avalonia.FFmpegVideoPlayer.csproj" />

<!-- Add this -->
<PackageReference Include="FFmpegVideoPlayer.Avalonia" Version="2.1.2" />
```
