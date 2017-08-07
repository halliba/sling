using System;

namespace Sling
{
    internal class AnnounceModel
    {
        public Guid Id { get; set; }

        public string Sender { get; set; }

        public string Filename { get; set; }

        public long FileSize { get; set; }

        public ushort Port { get; set; }
    }
}