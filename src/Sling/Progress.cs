using System;

namespace Sling
{
    public static class Progress
    {
        public static void Print(long total, long progress)
        {
            Console.Write($"\rProgress: {(double)progress /total:P} - {ToFileSize(progress)}/{ToFileSize(total)}");
        }

        public static string ToFileSize(long size)
        {
            if (size < 1024)
                return size.ToString("F0") + " B";

            if (size >> 10 < 1024)
                return (size / (float)1024).ToString("F2") + " KB";

            if (size >> 20 < 1024)
                return ((size >> 10) / (float)1024).ToString("F2") + " MB";

            if (size >> 30 < 1024)
                return ((size >> 20) / (float)1024).ToString("F2") + " GB";

            if (size >> 40 < 1024)
                return ((size >> 30) / (float)1024).ToString("F2") + " TB";

            if (size >> 50 < 1024)
                return ((size >> 40) / (float)1024).ToString("F2") + " PB";

            return ((size >> 50) / (float)1024).ToString("F0") + " EB";
        }
    }
}