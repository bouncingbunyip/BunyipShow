// File: BunyipShow/Core/Logger.cs
using System;
using System.IO;

namespace BunyipShow.Core
{
    public static class Logger
    {
        private static readonly object _lock = new();
        private static string? _currentLogFilePath;
        private static DateTime _currentLogDate;
        private static bool _enabled = true;

        public static void SetEnabled(bool enabled)
        {
            _enabled = enabled;
        }

        /// <summary>
        /// Provides a way to override the "current date" (useful for testing/log rolling simulation)
        /// </summary>
        public static Func<DateTime>? DateProvider { get; set; } = null;

        /// <summary>
        /// Logs a message with timestamp into a daily log file inside AppData\BunyipShow\log.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void Log(string message)
        {
            if (!_enabled) return;

            // Get the unique ID for THIS instance of the app
            int pid = Environment.ProcessId;
            // Get the current thread ID
            int tid = Environment.CurrentManagedThreadId;

            try
            {
                lock (_lock)
                {
                    UpdateLogFilePathIfNeeded();

                    string timestamp = (DateProvider?.Invoke() ?? DateTime.Now)
                        .ToString("yyyy-MM-dd HH:mm:ss");

                    string logEntry = $"[{timestamp}] [PID:{pid}] [TID:{tid}] {message}{Environment.NewLine}";
                    if (_currentLogFilePath != null)
                    {
                        File.AppendAllText(_currentLogFilePath, logEntry);
                    }
                }
            }
            catch
            {
                // Silent fail to avoid breaking the slideshow
            }
        }
        /// <summary>
        /// Ensures the log file path is up to date for the current date.
        /// Creates the log directory if it does not exist.
        /// </summary>
        private static void UpdateLogFilePathIfNeeded()
        {
            DateTime today = DateProvider?.Invoke().Date ?? DateTime.Today;

            if (_currentLogFilePath == null || _currentLogDate != today)
            {
                // ✅ AppData location
                string baseDir = ConfigHelper.GetAppDataFolder();
                string logDir = Path.Combine(baseDir, "log");

                Directory.CreateDirectory(logDir);

                string fileName = $"{today:yyyy-MM-dd}-bunyipshow.log";
                _currentLogFilePath = Path.Combine(logDir, fileName);
                _currentLogDate = today;
            }
        }
    }
}