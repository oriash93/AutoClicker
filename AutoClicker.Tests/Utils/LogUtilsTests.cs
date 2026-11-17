using System;
using System.IO;
using System.Linq;
using AutoClicker.Utils;
using Serilog.Events;
using Xunit;
using Logger = Serilog.Core.Logger;

namespace AutoClicker.Tests.Utils
{
    public class LogUtilsTests : IDisposable
    {
        private readonly string directoryPath = Path.Combine(Path.GetTempPath(), $"AutoClicker_Tests_{Guid.NewGuid()}");

        public void Dispose()
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }

        private string[] GetLogFileNames()
            => Directory.GetFiles(directoryPath, "AutoClicker_Logs*.txt")
                .Select(Path.GetFileName)
                .Order()
                .ToArray();

        private void WriteLog(LogEventLevel level, string message)
        {
            using Logger logger = LogUtils.CreateLogger(directoryPath, level);
            logger.Debug("Debug {Message}", message);
            logger.Information("Information {Message}", message);
        }

        [Fact]
        public void CreateLogger_WritesDatedFileInNewDirectory()
        {
            WriteLog(LogEventLevel.Debug, "test");

            Assert.Equal([$"AutoClicker_Logs{DateTime.Now:yyyyMMdd}.txt"], GetLogFileNames());
        }

        [Fact]
        public void CreateLogger_SkipsEventsBelowMinimumLevel()
        {
            WriteLog(LogEventLevel.Information, "test");

            string content = File.ReadAllText(Path.Combine(directoryPath, GetLogFileNames().Single()));
            Assert.Contains("[INF] Information test", content);
            Assert.DoesNotContain("[DBG]", content);
        }

        [Fact]
        public void CreateLogger_KeepsOnlyRetainedFileCount()
        {
            Directory.CreateDirectory(directoryPath);
            for (int day = 1; day <= 10; day++)
            {
                File.WriteAllText(Path.Combine(directoryPath, $"AutoClicker_Logs202001{day:00}.txt"), "old");
            }

            WriteLog(LogEventLevel.Debug, "test");

            string[] fileNames = GetLogFileNames();
            Assert.Equal(Constants.LOG_RETAINED_FILE_COUNT, fileNames.Length);
            Assert.Contains($"AutoClicker_Logs{DateTime.Now:yyyyMMdd}.txt", fileNames);
            Assert.DoesNotContain("AutoClicker_Logs20200101.txt", fileNames);
        }

        [Fact]
        public void CreateLogger_RollsOverAtFileSizeLimit()
        {
            using (Logger logger = LogUtils.CreateLogger(directoryPath, LogEventLevel.Debug, fileSizeLimitBytes: 1024))
            {
                for (int i = 0; i < 20; i++)
                {
                    logger.Information("Line {Index} {Padding}", i, new string('x', 100));
                }
            }

            string today = $"{DateTime.Now:yyyyMMdd}";
            string[] fileNames = GetLogFileNames();
            Assert.Contains($"AutoClicker_Logs{today}.txt", fileNames);
            Assert.Contains($"AutoClicker_Logs{today}_001.txt", fileNames);
            Assert.InRange(fileNames.Length, 2, Constants.LOG_RETAINED_FILE_COUNT);
            // The event that crosses the limit is still written, so a file can exceed it by one line
            Assert.All(fileNames, fileName => Assert.InRange(new FileInfo(Path.Combine(directoryPath, fileName)).Length, 1, 1024 + 256));
        }

        [Fact]
        public void DeleteLegacyLogFile_DeletesExistingFile()
        {
            Directory.CreateDirectory(directoryPath);
            string filePath = Path.Combine(directoryPath, "AutoClicker_Logs.txt");
            File.WriteAllText(filePath, "old");

            LogUtils.DeleteLegacyLogFile(filePath);

            Assert.False(File.Exists(filePath));
        }

        [Fact]
        public void DeleteLegacyLogFile_IgnoresMissingFile()
        {
            LogUtils.DeleteLegacyLogFile(Path.Combine(directoryPath, "AutoClicker_Logs.txt"));
        }
    }
}
