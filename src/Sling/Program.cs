using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security;
using Microsoft.Extensions.CommandLineUtils;

namespace Sling
{
    internal class Program : CommandLineApplication
    {
        private CommandOption SendOption { get; set; }

        private CommandOption ReceiveOption { get; set; }

        private CommandOption PortOption { get; set; }

        private CommandOption BufferOption { get; set; }

        private CommandArgument FileArgument { get; set; }
        
        private static int Main(string[] args)
        {
            var app = new Program
            {
                Name = "Sling",
                Description = $"Sling Command Line Utility (Version {Assembly.GetEntryAssembly().GetName().Version})"
                              + Environment.NewLine
                              + "Valentin Beller"
            };
            app.SendOption = app.Option("-s | --send", "Announces the specified file. (Implicit/Default)",
                CommandOptionType.NoValue);
            app.ReceiveOption = app.Option("-r | --receive", "Begin listening for files to receive.",
                CommandOptionType.NoValue);
            app.PortOption = app.Option("-p | --port", "Specifies the port used for file announcements.",
                CommandOptionType.SingleValue);
            app.BufferOption = app.Option("-b | --buffer", "Specified the buffer size used by the tcp stream.", CommandOptionType.SingleValue);

            app.FileArgument = app.Argument("Filepath", "Path to file, either read from or write to.");

            app.HelpOption("-? | -h | --help");
            app.OnExecute(() => app.InternalExecute());
            return app.Execute(args);
        }

        private int InternalExecute()
        {
            if (SendOption.HasValue() && ReceiveOption.HasValue())
            {
                Console.WriteLine("Use either -s or -r option.");
                return ReturnCodes.SendAndReceiveSpecified;
            }

            var isSend = !ReceiveOption.HasValue();
            var file = FileArgument.Value;
            if (isSend && string.IsNullOrEmpty(file))
            {
                Console.WriteLine("No file specified.");
                return ReturnCodes.NoFileSpecified;
            }
            if (!string.IsNullOrEmpty(file) && !CheckFile(file))
            {
                return ReturnCodes.InvalidFileSpecified;
            }

            ushort port = 56657;
            if (PortOption.HasValue())
            {
                var canPortParse = PortOption.HasValue() && ushort.TryParse(PortOption.Value(), out port);
                if (!canPortParse)
                {
                    Console.WriteLine("Invalid port.");
                    return ReturnCodes.InvalidPort;
                }
            }

            var bufferSize = 65536 * 32;
            if (BufferOption.HasValue())
            {
                var canBufferParse = BufferOption.HasValue() && int.TryParse(BufferOption.Value(), out bufferSize);
                if (!canBufferParse)
                {
                    Console.WriteLine("Invalid Buffer Size.");
                    return ReturnCodes.InvalidBufferSize;
                }
            }

            bool AcceptFileCallback(string name, long size, string sender, IPAddress source)
            {
                Console.Write($"File {name} ({Progress.ToFileSize(size)}) announced by {sender} ({source}), accept? (y/n) ");
                var read = Console.ReadLine();
                return read == "y";
            }

            var worker = isSend
                ? (Worker) new SendWorker(port, bufferSize, file)
                : new ReceiveWorker(port, bufferSize, file, AcceptFileCallback);
            worker.Run();

            return ReturnCodes.Ok;
        }
        
        private static bool CheckFile(string path)
        {
            try
            {
                if (File.Exists(path)) return true;
                Console.WriteLine($"File {path} does not exist.");
                return false;
            }
            catch (SecurityException)
            {
                Console.WriteLine($"Access to file {path} denied.");
                return false;
            }
            catch (Exception)
            {
                Console.WriteLine($"Can not read file {path}.");
                return false;
            }
        }
    }
}