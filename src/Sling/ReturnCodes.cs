namespace Sling
{
    internal static class ExitCodes
    {
        public const int UnknownError = -1;
        public const int Ok = 0;
        public const int SendAndReceiveSpecified = 1;
        public const int NoFileSpecified = 2;
        public const int InvalidFileSpecified = 3;
        public const int InvalidPort = 4;
        public const int InvalidBufferSize = 5;
        public const int RemoteStreamClosed = 6;
    }
}