using System.IO;
using System.Text.Json;
using Serilog;

namespace AutoClicker.Utils
{
    public static class JsonUtils
    {
        public static T ReadJson<T>(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    Log.Debug("Reading file = {FilePath}", filePath);
                    string jsonString = File.ReadAllText(filePath);
                    T result = JsonSerializer.Deserialize<T>(jsonString);
                    Log.Debug("Read from file successfully", filePath);
                    return result;
                }
                else
                {
                    Log.Warning("File {FilePath} is missing", filePath);
                    return default;
                }
            }
            catch (JsonException)
            {
                Log.Error("Failed parsing object of type {Type}", typeof(T));
                throw;
            }
        }

        public static void WriteJson<T>(string filePath, T data)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string jsonString = JsonSerializer.Serialize(data, options);
            using (StreamWriter streamWriter = File.CreateText(filePath))
            {
                streamWriter.Write(jsonString);
                Log.Debug("Write to file {FilePath} successfully", filePath);
            }
        }
    }
}
