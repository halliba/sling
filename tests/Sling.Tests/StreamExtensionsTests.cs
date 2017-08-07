using System.IO;
using System.Text;
using Xunit;

namespace Sling.Tests
{
    public class StreamExtensionsTests
    {
        private const string StreamTestData = "Sling is an awesome tool!";

        [Fact]
        public void ShouldCopyStreamSuccesful()
        {
            var sourceBytes = Encoding.Unicode.GetBytes(StreamTestData);
            var source = new MemoryStream(sourceBytes);
            var destination = new MemoryStream();

            source.CopyTo(destination, 16, sourceBytes.Length, l => { });

            var resultBytes = destination.ToArray();
            var result = Encoding.Unicode.GetString(resultBytes);

            Assert.Equal(StreamTestData, result);
        }
    }
}