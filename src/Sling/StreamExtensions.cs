using System;
using System.IO;

namespace Sling
{
    public static class StreamExtensions
    {
        public static void CopyTo(this Stream source, Stream destination, int bufferSize, long sourceBytes, Action<long> progressCallback)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (progressCallback == null) throw new ArgumentNullException(nameof(progressCallback));

            if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));
            if (sourceBytes < 0) throw new ArgumentOutOfRangeException(nameof(sourceBytes));

            var processed = 0L;
            progressCallback(processed);
            while (processed < sourceBytes)
            {
                var remaining = sourceBytes - processed;
                var length = (int)(remaining < bufferSize ? remaining : bufferSize);
                var data = new byte[length];
                var bytesRead = source.Read(data, 0, length);
                destination.Write(data, 0, bytesRead);
                processed += bytesRead;
                progressCallback(processed);
            }
        }
    }
}