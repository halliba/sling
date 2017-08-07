using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
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
            BeginAnnounceAsync();

            WaitAndSendFile();

            return ExitCodes.Ok;
        }

        private async void BeginAnnounceAsync()
        {
            var model = new AnnounceModel
            {
                Id = Guid.NewGuid(),
                Sender = Environment.MachineName,
                Filename = Path.GetFileName(FilePath),
                FileSize = new FileInfo(FilePath).Length,
                Port = _port
            };
            var jsonModel = JsonConvert.SerializeObject(model);
            var rawModel = Encoding.Unicode.GetBytes(jsonModel);
            var ipEndPoint = new IPEndPoint(IPAddress.Broadcast, Port);
            try
            {
                while (true)
                {
                    await _udpClient.SendAsync(rawModel, rawModel.Length, ipEndPoint);
                    await Task.Delay(700);
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void WaitAndSendFile()
        {
            _tcpListener = new TcpListener(new IPEndPoint(IPAddress.Any, _port));
            _tcpListener.Start();

            while (true)
            {
                var client = _tcpListener.AcceptTcpClientAsync().Result;
                InitJob(client);
            }
        }

        private async void InitJob(TcpClient client)
        {
            await Task.Run(() =>
            {
                using (client)
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

                        var progress = Progress.Outgoing(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString(), fileLength);
                        using (var fileStream = fileInfo.OpenRead())
                        {
                            fileStream.CopyTo(stream, BufferSize, fileLength, prog => progress.Update(prog));
                        }
                    }
                }
            });
        }

        private bool IsAccept(Stream stream, byte[] bytes)
        {
            if (BitConverter.ToUInt32(bytes, 0) != MagicNumber)
                throw new Exception("No magic number");

            var size = BitConverter.ToInt64(bytes, 4);
            var data = new byte[size];
            stream.Read(data, 0, (int) size);
            var rawText = Encoding.Unicode.GetString(data);
            var model = JsonConvert.DeserializeObject<AcceptModel>(rawText);
            if (!string.Equals(model.Filename, Path.GetFileName(FilePath), StringComparison.Ordinal))
                throw new Exception("wrong fileName");
            return true;
        }
    }
}