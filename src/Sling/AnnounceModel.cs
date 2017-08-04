namespace Sling
{
    internal class AnnounceModel
    {
        public string Sender { get; set; }

        public string Filename { get; set; }

        public long FileSize { get; set; }

        public short Port { get; set; }
    }
}