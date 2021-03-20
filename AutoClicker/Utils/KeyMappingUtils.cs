using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AutoClicker.Utils
{
    public static class KeyMappingUtils
    {
        private static readonly string keysMappingPath =
            Path.Combine(Environment.CurrentDirectory, Constants.RESOURCES_DIRECTORY, Constants.KEY_MAPPINGS_RESOURCE_PATH);
       
        private static List<KeyMapping> keyMapping;

        public static List<KeyMapping> KeyMapping
        {
            get
            {
                if (keyMapping == null)
                {
                    LoadMapping();
                }
                return keyMapping;
            }
            set
            {
                keyMapping = value;
            }
        }

        private static void LoadMapping()
        {
            try
            {
                if (File.Exists(keysMappingPath))
                {
                    string jsonString = File.ReadAllText(keysMappingPath);
                    keyMapping = JsonSerializer.Deserialize<List<KeyMapping>>(jsonString);
                }
                else
                {
                    // TODO: log this exception or show error message
                }
            }
            catch (JsonException)
            {
                // TODO: log this exception or show error message
            }
        }

    }
}
