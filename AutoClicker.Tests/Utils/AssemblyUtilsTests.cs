using System;
using System.IO;
using AutoClicker.Utils;
using Xunit;

namespace AutoClicker.Tests.Utils
{
    public class AssemblyUtilsTests
    {
        [Fact]
        public void GetAssemblyInfo_ReturnsAppAssembly()
        {
            Assert.Equal("AutoClicker", AssemblyUtils.GetAssemblyInfo().Name);
        }

        [Fact]
        public void GetDisplayVersion_HasNoBuildMetadata()
        {
            string version = AssemblyUtils.GetDisplayVersion();

            Assert.False(string.IsNullOrWhiteSpace(version));
            Assert.DoesNotContain("+", version);
        }

        [Fact]
        public void GetProjectUri_ReturnsRepositoryUrl()
        {
            Assert.Equal(new Uri("https://github.com/oriash93/AutoClicker"), AssemblyUtils.GetProjectUri());
        }

        [Fact]
        public void GetResourceStream_ReturnsEmbeddedKeyMappings()
        {
            using Stream stream = AssemblyUtils.GetResourceStream(Constants.KEY_MAPPINGS_RESOURCE_NAME);

            Assert.NotNull(stream);
            Assert.True(stream.Length > 0);
        }

        [Fact]
        public void GetResourceStream_ReturnsNullForUnknownResource()
        {
            Assert.Null(AssemblyUtils.GetResourceStream("missing.json"));
        }
    }
}
