using System;
using System.Collections.Generic;
using System.IO;
using AutoClicker.Models;

namespace AutoClicker.Utils
{
    public static class KeyMappingUtils
    {
        private static readonly string keysMappingPath =
            Path.Combine(AppContext.BaseDirectory,Constants.RESOURCES_DIRECTORY, Constants.KEY_MAPPINGS_RESOURCE_PATH);

        public static List<KeyMapping> KeyMapping { get; set; }

        static KeyMappingUtils()
        {
            LoadMapping();
        }

        private static void LoadMapping()
        {
            if (KeyMapping != null)
                return;
            ReadMapping();
        }

        public static KeyMapping GetKeyMappingByCode(int virtualKeyCode)
        {
            return KeyMapping.Find(keyMapping => keyMapping.VirtualKeyCode == virtualKeyCode);
        }

        public static string GetDisplayName(KeyMapping keyMapping)
        {
            if (!IsKeyboardLayoutDependent(keyMapping.VirtualKeyCode))
                return keyMapping.DisplayName;

            nint keyboardLayout = Win32ApiUtils.GetKeyboardLayout(0);
            uint result = Win32ApiUtils.MapVirtualKey((uint)keyMapping.VirtualKeyCode, Constants.MAPVK_VK_TO_CHAR, keyboardLayout);

            // The top bit marks dead keys
            char character = (char)(result & 0xFFFF);
            return character == 0 ? keyMapping.DisplayName : character.ToString();
        }

        private static bool IsKeyboardLayoutDependent(int virtualKeyCode)
        {
            return (virtualKeyCode >= Constants.VK_0 && virtualKeyCode <= Constants.VK_9)
                || (virtualKeyCode >= Constants.VK_OEM_1 && virtualKeyCode <= Constants.VK_OEM_3)
                || (virtualKeyCode >= Constants.VK_OEM_4 && virtualKeyCode <= Constants.VK_OEM_7);
        }

        private static void ReadMapping()
        {
            KeyMapping = JsonUtils.ReadJson<List<KeyMapping>>(keysMappingPath);
        }
    }
}
