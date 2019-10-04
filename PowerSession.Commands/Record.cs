namespace PowerSession.Commands
{
    using System;
    using System.Collections;
    using System.IO;
    using ConPTY;
    using Newtonsoft.Json;
    using Utils;

    public struct OutputHeader
    {
        [JsonProperty("version")] public int Version;
        [JsonProperty("width")] public int Width;
        [JsonProperty("height")] public int Height;
        [JsonProperty("timestamp")] public long Timestamp;
        [JsonProperty("env")] public IDictionary Environment;
    }

    public struct RecordArgs
    {
        public string Filename;
        public string Command;
        public bool Overwrite;

        public RecordArgs(string filename, string command = null, bool overwrite = false)
        {
            Filename = filename;
            Command = command;
            Overwrite = overwrite;
        }
    }

    public class RecordCommand : ICommand
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

        private void _record(string filename, string command = null)
        {
            if (string.IsNullOrEmpty(command)) command = "powershell.exe";

            if (_env == null) _env = Environment.GetEnvironmentVariables();
            _env.Add("POWERSESSION_RECORDING", "1");

            using var writer = new FileWriter(filename);

            var terminal = new Terminal(writer.GetInputStream(), writer.GetWriteStream());

            writer.SetHeader(new OutputHeader
            {
                Version = 2,
                Width = terminal.Width,
                Height = terminal.Height,
                Environment = _env
            });
            terminal.Record(command, _env);
            Console.WriteLine("Record Finished");
        }
    }
}