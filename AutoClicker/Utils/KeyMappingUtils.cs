using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Serilog;

namespace AutoClicker.Utils
{
    public static class KeyMappingUtils
    {
        private static readonly string keysMappingPath =
            Path.Combine(Environment.CurrentDirectory, Constants.RESOURCES_DIRECTORY, Constants.KEY_MAPPINGS_RESOURCE_PATH);

        public static List<KeyMapping> KeyMapping { get; set; }

        static KeyMappingUtils()
        {
            LoadMapping();
        }

        private static void LoadMapping()
        {
            if (KeyMapping == null)
            {
                ReadMapping();
            }
        }

        public static KeyMapping GetKeyMappingByCode(int virtualKeyCode)
        {
            return KeyMapping.Find(keyMapping => keyMapping.VirtualKeyCode == virtualKeyCode);
        }

        private static void ReadMapping()
        {
            try
            {
                if (File.Exists(keysMappingPath))
                {
                    string jsonString = File.ReadAllText(keysMappingPath);
                    KeyMapping = JsonSerializer.Deserialize<List<KeyMapping>>(jsonString);
                }
                else
                {
                    Log.Error("File {FilePath} is missing", keysMappingPath);
                    throw new FileNotFoundException(keysMappingPath);
                }
            }
            catch (JsonException)
            {
                Log.Warning("Failed parsing KeyMapping");
            }
            Log.Debug("Read file {FilePath} succesfully", keysMappingPath);
        }

    }
}
