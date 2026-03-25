namespace FFmpegVideoPlayer.Core;

/// <summary>
/// Interface for video frame rendering implementations.
/// Allows different rendering backends (CPU, OpenGL, etc.) to be used.
/// </summary>
public interface IVideoRenderer : IDisposable
{
    /// <summary>
    /// Gets or sets the video width.
    /// </summary>
    int Width { get; set; }

    /// <summary>
    /// Gets or sets the video height.
    /// </summary>
    int Height { get; set; }

    /// <summary>
    /// Renders a video frame.
    /// </summary>
    /// <param name="frameData">Raw BGRA frame data.</param>
    /// <param name="width">Frame width.</param>
    /// <param name="height">Frame height.</param>
    /// <param name="stride">Bytes per row.</param>
    void RenderFrame(IntPtr frameData, int width, int height, int stride);

    /// <summary>
    /// Renders a video frame from a byte array.
    /// </summary>
    /// <param name="frameData">Raw BGRA frame data.</param>
    /// <param name="width">Frame width.</param>
    /// <param name="height">Frame height.</param>
    /// <param name="stride">Bytes per row.</param>
    void RenderFrame(byte[] frameData, int width, int height, int stride);

    /// <summary>
    /// Clears the renderer (e.g., shows black screen).
    /// </summary>
    void Clear();
}

