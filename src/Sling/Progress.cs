using System;

namespace Sling
{
    public class Progress
    {
        private readonly string _to;
        private readonly long _total;
        private readonly int _cursorTop;
        private readonly bool _incoming;

        private long _progress;

        private Progress(bool incoming, string to, long total, int cursorTop)
        {
            _incoming = incoming;
            _to = to;
            _total = total;
            _cursorTop = cursorTop;
        }

        public static Progress Incoming(string sender, long total)
        {
            var progress = new Progress(true, sender, total, Console.CursorTop);
            progress.Print();
            Console.WriteLine();
            return progress;
        }

        public static Progress Outgoing(string receipient, long total)
        {
            var progress = new Progress(false, receipient, total, Console.CursorTop);
            progress.Print();
            Console.WriteLine();
            return progress;
        }

        public void Update(long newProgress)
        {
            _progress = newProgress;
            Print();
        }
        
        private void Print()
        {
            lock (Console.Out)
            {
                var oldLeft = Console.CursorLeft;
                var oldTop = Console.CursorTop;
                Console.SetCursorPosition(0, _cursorTop);
                Console.Write($"{(_incoming ? "Retrieving from:" : "Send to:")} {_to}: {(double)_progress / _total:P} - {ToFileSize(_progress)}/{ToFileSize(_total)}");
                Console.SetCursorPosition(oldLeft, oldTop);
            }
        }

        public static string ToFileSize(long size, bool useBinaryMode = false)
        {
            string[] suffixes =
            {
                "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB",
                "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB"
            };

            int s;
            double factor;
            if (useBinaryMode)
            {
                s = 7;
                factor = 1024;
            }
            else
            {
                s = -1;
                factor = 1000;
            }

            double value = size;
            if(value <= factor)
                return $"{value} B";

            do
            {
                s++;
                value /= factor;
            } while (value >= factor);

            return $"{value:F2} {suffixes[s]}";
        }
    }
}