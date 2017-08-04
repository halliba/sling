using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace Sling
{
    public class SendWorker : Worker
    {
        private readonly UdpClient _udpClient;
        private TcpListener _tcpListener;
        private readonly ushort _port = (ushort) new Random().Next(0xC000, 0xFFFF + 1);

        public SendWorker(ushort port, int bufferSize, string filePath) : base(port, bufferSize, filePath)
        {
            _udpClient = new UdpClient();
        }

        public override int Run()
        {
            var code = Announce();
            if (code != 0)
                return code;

            code = WaitAndSendFile();
            return code;
        }

        private int Announce()
        {
            var model = new AnnounceModel
            {
                Sender = Environment.MachineName,
                Filename = Path.GetFileName(FilePath),
                FileSize = new FileInfo(FilePath).Length,
                Port = _port
            };
            var jsonModel = JsonConvert.SerializeObject(model);
            var rawModel = Encoding.Unicode.GetBytes(jsonModel);
            _udpClient.SendAsync(rawModel, rawModel.Length, new IPEndPoint(IPAddress.Broadcast, Port));
            return ExitCodes.Ok;
        }

        private int WaitAndSendFile()
        {
            _tcpListener = new TcpListener(new IPEndPoint(IPAddress.Any, _port));
            _tcpListener.Start();

            using (var client = _tcpListener.AcceptTcpClientAsync().Result)
            {
                using (var stream = client.GetStream())
                {
                    var bytes = new byte[12];
                    stream.Read(bytes, 0, 12);
                    if (!IsAccept(stream, bytes))
                        throw new Exception("no accept");

                    var fileInfo = new FileInfo(FilePath);
                    var fileLength = fileInfo.Length;
                    var prefix = BitConverter.GetBytes(MagicNumber).Concat(BitConverter.GetBytes(fileLength)).ToArray();
                    stream.Write(prefix, 0, 12);

                    using (var fileStream = fileInfo.OpenRead())
                    {
                        fileStream.CopyTo(stream, BufferSize, fileLength, prog => Progress.Print(fileLength, prog));
                    }
                }
            }
            _tcpListener.Stop();

            return ExitCodes.Ok;
        }

        private bool IsAccept(Stream stream, byte[] bytes)
        {
            if (BitConverter.ToUInt32(bytes, 0) != MagicNumber)
                throw new Exception("No magic number");

            var size = BitConverter.ToInt64(bytes, 4);
            var data = new byte[size];
            stream.Read(data, 0, (int)size);
            var rawText = Encoding.Unicode.GetString(data);
            var model = JsonConvert.DeserializeObject<AcceptModel>(rawText);
            if (!string.Equals(model.Filename, Path.GetFileName(FilePath), StringComparison.Ordinal))
                throw new Exception("wrong fileName");
            return true;
        }
    }
}