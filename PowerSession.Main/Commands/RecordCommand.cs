namespace PowerSession.Main.Commands
{
    using ConPTY;
    using Newtonsoft.Json;
    using PowerSession.Commands;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using Utils;

    public struct RecordHeader
    {
        [JsonProperty("version")] public int Version;
        [JsonProperty("width")] public int Width;
        [JsonProperty("height")] public int Height;
        [JsonProperty("timestamp")] public long Timestamp;
        [JsonProperty("env")] public IDictionary Environment;
        public bool Valid => Version == 2;
    }

    public struct RecordArgs
    {
        public string Filename;
        public string Command;
        public bool Overwrite;
    }

    public class RecordCommand : BaseCommand, ICommand
    {
        private readonly RecordArgs _args;
        private readonly string _command;
        private IDictionary _env;
        private string _filename;

        public RecordCommand(RecordArgs args, IDictionary env = null)
        {
            _filename = args.Filename;
            _command = args.Command;
            _env = env;
            _args = args;
        }

        public int Execute()
        {
            if (string.IsNullOrEmpty(_filename)) _filename = Path.GetTempFileName();

            if (File.Exists(_filename))
                if (_args.Overwrite)
                    File.Delete(_filename);

            var fd = File.Create(_filename);
            fd.Dispose();

            try
            {
                _record(_filename, _command);
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static string _getTerm()
        {
            return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WT_SESSION")) ? "windows-terminal" :
                Environment.GetEnvironmentVariable("TERM");
        }

        private void _record(string filename, string command = null)
        {
            command ??= Environment.GetEnvironmentVariable("SHELL") ?? "powershell.exe";

            _env ??= new Dictionary<string, string>();
            _env.Add("SHELL", Environment.GetEnvironmentVariable("SHELL") ?? "powershell.exe");

            string term = _getTerm();
            if (!string.IsNullOrWhiteSpace(term))
            {
                _env.Add("TERM", term);
            }

            using var writer = new FileWriter(filename);
            var headerInfo = new RecordHeader
            {
                Version = 2,
                Width = Console.WindowWidth,
                Height = Console.WindowHeight,
                Environment = _env
            };
            writer.SetHeader(headerInfo);

            var terminal = new Terminal(writer.GetInputStream(), writer.GetWriteStream(), width: headerInfo.Width, height: headerInfo.Height);
            terminal.Record(command, _env);
            Console.WriteLine("Record Finished");
        }
    }
}