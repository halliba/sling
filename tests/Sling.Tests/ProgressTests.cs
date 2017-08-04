using Xunit;

namespace Sling.Tests
{
    public class ProgressTests
    {
        [Theory]
        [InlineData(0, "0 B")]
        public void ShouldFormatSize(long value, string result)
        {
            var formatted = Progress.ToFileSize(value);

            Assert.Equal(result, formatted);
        }
    }
}