namespace FFmpegVideoPlayer.Core;

/// <summary>
/// Interface for audio playback implementations.
/// Allows the core player to work with different audio backends or no audio at all.
/// </summary>
public interface IAudioPlayer : IDisposable
{
    /// <summary>
    /// Sets the volume level (0.0 to 1.0).
    /// </summary>
    void SetVolume(float volume);

    /// <summary>
    /// Resumes audio playback.
    /// </summary>
    void Resume();

    /// <summary>
    /// Pauses audio playback.
    /// </summary>
    void Pause();

    /// <summary>
    /// Stops audio playback and clears buffers.
    /// </summary>
    void Stop();

    /// <summary>
    /// Gets the current audio playback time in seconds based on samples played.
    /// Used for audio-video synchronization.
    /// </summary>
    double GetPlaybackTime();

    /// <summary>
    /// Queues pre-converted S16 stereo samples directly.
    /// More efficient when using SwrContext for resampling.
    /// </summary>
    /// <param name="samples">Pointer to S16 sample data (stereo interleaved: L, R, L, R, ...).</param>
    /// <param name="sampleCount">Total number of samples (for stereo: left samples + right samples). 
    /// For stereo, this equals the number of stereo pairs * 2.</param>
    unsafe void QueueSamplesS16(short* samples, int sampleCount);

    /// <summary>
    /// Queues float samples (will be converted internally if needed).
    /// </summary>
    /// <param name="samples">Float samples array (stereo interleaved).</param>
    void QueueSamples(float[] samples);
}

