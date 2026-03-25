using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace FFmpegVideoPlayer.Core;

/// <summary>
/// JSON logger for player operations. Logs all player commands and state changes to a JSON file.
/// </summary>
public sealed class PlayerLogger : IDisposable
{
    private readonly List<LogEntry> _logEntries = new();
    private readonly object _lock = new();
    private readonly string _logFilePath;
    private bool _disposed;
    private DateTime _lastFlush = DateTime.Now;

    public PlayerLogger(string? logFilePath = null)
    {
        if (string.IsNullOrEmpty(logFilePath))
        {
            var logDir = Path.Combine(AppContext.BaseDirectory, "debug");
            Directory.CreateDirectory(logDir);
            _logFilePath = Path.Combine(logDir, $"player-log-{DateTime.Now:yyyyMMdd-HHmmss}.json");
        }
        else
        {
            _logFilePath = logFilePath;
        }

        Log("PlayerLogger", "Initialized", new { LogFile = _logFilePath });
    }

    public void Log(string component, string operation, object? data = null)
    {
        lock (_lock)
        {
            if (_disposed) return;

            var entry = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                TimestampLocal = DateTime.Now,
                Component = component,
                Operation = operation,
                Data = data
            };

            _logEntries.Add(entry);

            // Also write to debug output for immediate visibility
            var dataStr = data != null ? $" | Data: {JsonSerializer.Serialize(data)}" : "";
            Debug.WriteLine($"[{entry.TimestampLocal:HH:mm:ss.fff}] [{component}] {operation}{dataStr}");
            
            // Auto-flush every 5 seconds to ensure logs are saved even if app crashes
            if ((DateTime.Now - _lastFlush).TotalSeconds > 5)
            {
                Flush();
            }
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            if (_disposed) return;
            
            var clearedCount = _logEntries.Count;
            _logEntries.Clear();
            
            // Write empty array to file to clear it
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var json = JsonSerializer.Serialize(new List<LogEntry>(), options);
                File.WriteAllText(_logFilePath, json, Encoding.UTF8);
                _lastFlush = DateTime.Now;
                
                Debug.WriteLine($"[PlayerLogger] Cleared {clearedCount} log entries from {_logFilePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PlayerLogger] Failed to clear log file: {ex.Message}");
            }
        }
    }

    public void Flush()
    {
        lock (_lock)
        {
            if (_disposed || _logEntries.Count == 0) return;

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(_logEntries, options);
                File.WriteAllText(_logFilePath, json, Encoding.UTF8);
                _lastFlush = DateTime.Now;
                
                Debug.WriteLine($"[PlayerLogger] Flushed {_logEntries.Count} log entries to {_logFilePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PlayerLogger] Failed to write log file: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Flush();
    }

    private class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public DateTime TimestampLocal { get; set; }
        public string Component { get; set; } = "";
        public string Operation { get; set; } = "";
        public object? Data { get; set; }
    }
}

