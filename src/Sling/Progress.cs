using System;

namespace Sling
{
    public static class Progress
    {
        public static void Print(long total, long progress)
        {
            Console.Write($"\rProgress: {(double)progress /total:P} - {ToFileSize(progress)}/{ToFileSize(total)}");
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