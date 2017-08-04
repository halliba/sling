using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace Sling
{
    public delegate bool AcceptFileDelegate(string name, long size, string sender, IPAddress source);

    public class ReceiveWorker : Worker
    {
        private readonly UdpClient _udpClient;
        private AnnounceModel _model;

        private readonly AcceptFileDelegate _acceptFileCallback;

        public ReceiveWorker(ushort port, int bufferSize, string filePath, AcceptFileDelegate acceptFileCallback) : base(port, bufferSize, filePath)
        {
            _acceptFileCallback = acceptFileCallback ?? throw new ArgumentNullException(nameof(acceptFileCallback));
            _udpClient = new UdpClient(port);
        }

        public override int Run()
        {
            var waitForFile = true;
            IPEndPoint remoteEndPoint = null;
            while (waitForFile)
            {
                var received = _udpClient.ReceiveAsync().Result;
                var rawText = Encoding.Unicode.GetString(received.Buffer);
                _model = JsonConvert.DeserializeObject<AnnounceModel>(rawText);

                remoteEndPoint = received.RemoteEndPoint;
                var sender = _model.Sender ?? Dns.GetHostEntryAsync(remoteEndPoint.Address).Result.HostName;

                var accepted = _acceptFileCallback(_model.Filename, _model.FileSize, sender, remoteEndPoint.Address);
                if (accepted) waitForFile = false;
            }

            if (FilePath == null) FilePath = _model.Filename;
            using (var tcpClient = new TcpClient())
            {
                tcpClient.ConnectAsync(remoteEndPoint.Address, _model.Port).Wait();
                try
                {
                    using (var stream = tcpClient.GetStream())
                    {
                        var code = SendAccept(stream);
                        if (code != ExitCodes.Ok)
                            return code;

                        code = ReceiveFile(stream);
                        return code;
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine("Can not read from remote source: " + e.Message);
                    return ExitCodes.RemoteStreamClosed;
                }
            }
        }

        private int SendAccept(Stream stream)
        {
            var model = new AcceptModel {Filename = _model.Filename};
            var rawText = JsonConvert.SerializeObject(model);
            var rawData = Encoding.Unicode.GetBytes(rawText);
            var prefix = BitConverter.GetBytes(MagicNumber).Concat(BitConverter.GetBytes(rawData.Length)).ToArray();
            stream.Write(prefix, 0, prefix.Length);
            stream.Write(rawData, 0, rawData.Length);

            return ExitCodes.Ok;
        }

        private int ReceiveFile(Stream stream)
        {
            var bytes = new byte[12];
            stream.Read(bytes, 0, 12);
            if (BitConverter.ToUInt32(bytes, 0) != MagicNumber)
                throw new Exception("No magic number");
            var fileLength = BitConverter.ToInt64(bytes, 4);

            using (var fileStream = File.Open(FilePath, FileMode.Create))
            {
                stream.CopyTo(fileStream, BufferSize, fileLength, prog => Progress.Print(fileLength, prog));
            }
            return ExitCodes.Ok;
        }
    }
}