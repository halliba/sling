using System;

namespace Sling
{
    internal abstract class Worker
    {
        protected readonly ushort Port;
        protected readonly int BufferSize;
        protected string FilePath;
        protected const uint MagicNumber = 0x73_6C_6E_67;

        protected Worker(ushort port, int bufferSize, string filePath)
        {
            if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));
            Port = port;
            BufferSize = bufferSize;
            FilePath = filePath;
        }

        public abstract int Run();
    }
}