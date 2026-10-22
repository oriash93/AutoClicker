using System.Linq;
using AutoClicker.Utils;
using Xunit;

namespace AutoClicker.Tests.Utils
{
    public class KeyMappingUtilsTests
    {
        [Fact]
        public void KeyMapping_IsLoaded()
        {
            Assert.NotNull(KeyMappingUtils.KeyMapping);
            Assert.NotEmpty(KeyMappingUtils.KeyMapping);
        }

        [Fact]
        public void KeyMapping_EntriesAreComplete()
        {
            Assert.All(KeyMappingUtils.KeyMapping, keyMapping =>
            {
                Assert.False(string.IsNullOrWhiteSpace(keyMapping.VirtualKeyName));
                Assert.False(string.IsNullOrWhiteSpace(keyMapping.DisplayName));
                Assert.InRange(keyMapping.VirtualKeyCode, 0x01, 0xFE);
            });
        }

        [Fact]
        public void KeyMapping_HasNoDuplicateVirtualKeyCodes()
        {
            int[] duplicates = KeyMappingUtils.KeyMapping
                .GroupBy(keyMapping => keyMapping.VirtualKeyCode)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToArray();

            Assert.Empty(duplicates);
        }

        [Theory]
        [InlineData(Constants.DEFAULT_START_HOTKEY)]
        [InlineData(Constants.DEFAULT_STOP_HOTKEY)]
        [InlineData(Constants.DEFAULT_TOGGLE_HOTKEY)]
        [InlineData(Constants.VK_OEM_8)]
        [InlineData(Constants.VK_OEM_102)]
        public void GetKeyMappingByCode_FindsKey(int virtualKeyCode)
        {
            Assert.NotNull(KeyMappingUtils.GetKeyMappingByCode(virtualKeyCode));
        }
    }
}
