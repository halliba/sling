﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace Sling
{
    internal class SendWorker : Worker
    {
        private readonly UdpClient _udpClient;
        private TcpListener _tcpListener;
        private readonly short _port = (short) new Random().Next(0, short.MaxValue + 1);

        public SendWorker(ushort port, int bufferSize, string filePath) : base(port, bufferSize, filePath)
        {
            _udpClient = new UdpClient();
        }

        public override void Run()
        {
            Announce();
            WaitAndSendFile();
        }

        private void Announce()
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
        }

        private void WaitAndSendFile()
        {
            _tcpListener = new TcpListener(new IPEndPoint(IPAddress.Any, _port));
            _tcpListener.Start();

            var client = _tcpListener.AcceptTcpClientAsync().Result;
            var stream = client.GetStream();
            var bytes = new byte[12];
            stream.Read(bytes, 0, 12);
            if(!IsAccept(stream, bytes))
                throw new Exception("no accept");

            var fileInfo = new FileInfo(FilePath);
            var fileLength = fileInfo.Length;
            var prefix = BitConverter.GetBytes(MagicNumber).Concat(BitConverter.GetBytes(fileLength)).ToArray();
            stream.Write(prefix, 0, 12);

            var fileStream = fileInfo.OpenRead();
            fileStream.CopyTo(stream, BufferSize, fileLength, prog => Progress.Print(fileLength, prog));

            stream.Dispose();
            fileStream.Dispose();
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