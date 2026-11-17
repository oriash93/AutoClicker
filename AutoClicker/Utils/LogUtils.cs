using System;
using System.IO;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace AutoClicker.Utils
{
    public static class LogUtils
    {
        private static readonly string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string logDirectoryPath = Path.Combine(appDataPath, Constants.LOG_DIRECTORY_PATH);
        private static readonly string legacyLogFilePath = Path.Combine(appDataPath, Constants.LEGACY_LOG_FILE_PATH);

#if DEBUG
        private const LogEventLevel minimumLevel = LogEventLevel.Debug;
#else
        private const LogEventLevel minimumLevel = LogEventLevel.Information;
#endif

        public static void Initialize()
        {
            Log.Logger = CreateLogger(logDirectoryPath, minimumLevel);
            Log.Debug("==================================================");
            Log.Information("Logger initialized successfully");

            DeleteLegacyLogFile(legacyLogFilePath);
        }

        public static Logger CreateLogger(string directoryPath, LogEventLevel level, long fileSizeLimitBytes = Constants.LOG_FILE_SIZE_LIMIT_BYTES)
            => new LoggerConfiguration()
                .MinimumLevel.Is(level)
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(directoryPath, Constants.LOG_FILE_NAME),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: Constants.LOG_RETAINED_FILE_COUNT,
                    fileSizeLimitBytes: fileSizeLimitBytes,
                    rollOnFileSizeLimit: true)
                .CreateLogger();

        public static void DeleteLegacyLogFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            try
            {
                File.Delete(filePath);
                Log.Information("Deleted legacy log file {FilePath}", filePath);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                Log.Warning(e, "Failed to delete legacy log file {FilePath}", filePath);
            }
        }
    }
}
