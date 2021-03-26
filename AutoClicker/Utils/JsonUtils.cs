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
                    Log.Debug("Read file {FilePath}", filePath);
                    string jsonString = File.ReadAllText(filePath);
                    T result = JsonSerializer.Deserialize<T>(jsonString);
                    Log.Debug("Read from file {FilePath} successfully", filePath);
                    return result;
                }
                else
                {
                    Log.Error("File {FilePath} is missing", filePath);
                    throw new FileNotFoundException(filePath);
                }
            }
            catch (JsonException)
            {
                Log.Warning("Failed parsing object of type {Type}", typeof(T));
                throw;
            }
        }

        public static void WriteJson<T>(string filePath, T data)
        {
            string jsonString = JsonSerializer.Serialize(data);
            using (StreamWriter streamWriter = File.CreateText(filePath))
            {
                streamWriter.Write(jsonString);
                Log.Debug("Write to file {FilePath} successfully", filePath);
            }
        }
    }
}
