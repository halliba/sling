using System;
using Xunit;

namespace Sling.Tests
{
    public class WorkerTests
    {
        private class WorkerHelpClass : Worker
        {
            public WorkerHelpClass(ushort port, int bufferSize, string filePath) : base(port, bufferSize, filePath)
            {
                
            }

            public override int Run()
            {
                throw new NotImplementedException();
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void ShouldThrowOnIllegalBufferSize(int bufferSize)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // ReSharper disable once ObjectCreationAsStatement
                new WorkerHelpClass(0, bufferSize, null);
            });
        }
    }
}