namespace PowerSession.Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;

    struct SessionLine
    {
        public double Timestamp;
        public bool Stdout;
        public string Content;
    }

    internal class RecordSession : IDisposable
    {
        private string _filepath;
        private readonly RecordHeader _header;
        private readonly StreamReader _reader;

        internal RecordSession(string filepath)
        {
            _filepath = filepath;
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException(filepath);
            }

            _reader = new StreamReader(File.OpenRead(filepath));File.OpenRead(filepath);
            
            var headerLine = _reader.ReadLine();
            _header = JsonConvert.DeserializeObject<RecordHeader>(headerLine);
            if (!_header.Valid)
            {
                throw new InvalidDataException("Unsupported file format");
            }
        }

        internal IEnumerable<SessionLine> Lines()
        {
            while (!_reader.EndOfStream)
            {
                var line = _reader.ReadLine();
                var lineData = JsonConvert.DeserializeObject<List<object>>(line);
                if (lineData.Count != 3) throw new InvalidDataException("Invalid record data");
                var rv = new SessionLine
                {
                    Timestamp = (double) lineData[0],
                    Stdout = (string) lineData[1] == "o",
                    Content = (string) lineData[2]
                };
                yield return rv;
            }
        }

        internal IEnumerable<SessionLine> Stdout()
        {
            return Lines().Where(line => line.Stdout);
        }

        internal IEnumerable<SessionLine> StdoutRelativeTime()
        {
            double previousTimestamp = 0;

            foreach (var line in Stdout())
            {
                var newLine = new SessionLine
                {
                    Timestamp = line.Timestamp - previousTimestamp, Content = line.Content, Stdout = line.Stdout
                };
                previousTimestamp = line.Timestamp;
                yield return newLine;
            }
        }

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}