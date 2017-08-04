using Xunit;

namespace Sling.Tests
{
    public class ProgressTests
    {
        [Theory]
        [InlineData(0, "0 B")]
        [InlineData(388, "388 B")]
        [InlineData(38817, "38,82 KB")]
        [InlineData(8512487, "8,51 MB")]
        [InlineData(3881799132, "3,88 GB")]
        [InlineData(3881799132391, "3,88 TB")]
        [InlineData(4876544265565669, "4,88 PB")]
        [InlineData(5431486455442159594, "5,43 EB")]
        public void ShouldFormatSize(long value, string result)
        {
            var formatted = Progress.ToFileSize(value);

            Assert.Equal(result, formatted);
        }

        [Theory]
        [InlineData(0, "0 B")]
        [InlineData(388, "388 B")]
        [InlineData(38817, "37,91 KiB")]
        [InlineData(8512487, "8,12 MiB")]
        [InlineData(3881799132, "3,62 GiB")]
        [InlineData(3881799132391, "3,53 TiB")]
        [InlineData(4876544265565669, "4,33 PiB")]
        [InlineData(5431486455442159594, "4,71 EiB")]
        public void ShouldFormatSizeInBinaryMode(long value, string result)
        {
            var formatted = Progress.ToFileSize(value, true);

            Assert.Equal(result, formatted);
        }
    }
}