namespace PowerSession.Commands.Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Pipes;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class FileWriter : IDisposable
    {
        private static readonly ManualResetEvent WaitForWriter = new ManualResetEvent(false);
        private readonly FileStream _fileStream;
        private readonly DateTimeOffset _startTimeStamp;
        private OutputHeader _header;

        public FileWriter(string output)
        {
            if (!File.Exists(output)) throw new FileNotFoundException(output);
            _fileStream = File.OpenWrite(output);
            _startTimeStamp = DateTimeOffset.Now;
        }

        public void Dispose()
        {
            WaitForWriter.WaitOne();
            _fileStream?.Dispose();
        }

        public void SetHeader(OutputHeader header)
        {
            header.Timestamp = _startTimeStamp.ToUnixTimeMilliseconds();
            _header = header;
        }

        /// <summary>
        ///     Record stdin
        /// </summary>
        /// <returns></returns>
        public Stream GetInputStream()
        {
            var pipeServer = new AnonymousPipeServerStream();
            var pipeClient = new AnonymousPipeClientStream(pipeServer.GetClientHandleAsString());

            return pipeClient;
        }

        public Stream GetWriteStream()
        {
            var pipeServer = new AnonymousPipeServerStream();
            var pipeClient = new AnonymousPipeClientStream(pipeServer.GetClientHandleAsString());

            Task.Factory.StartNew(() =>
            {
                using var sr = new StreamReader(pipeClient);
                using var sw = new StreamWriter(_fileStream, new UTF8Encoding(false));
                sw.WriteLine(JsonConvert.SerializeObject(_header));
                var buf = new char[1024];
                int bytesRead;
                while ((bytesRead = sr.Read(buf, 0, buf.Length)) != 0)
                {
                    var ts = DateTimeOffset.Now - _startTimeStamp;
                    var chars = string.Join("", buf.Take(bytesRead).Select(c => c.ToString()));
                    var data = new List<object> {ts.TotalMilliseconds / 1000, "o", chars};
                    sw.WriteLine(JsonConvert.SerializeObject(data)); // asciinema compatible
                    Console.Out.Write(buf.Take(bytesRead).ToArray());
                }

                WaitForWriter.Set();
            }, TaskCreationOptions.LongRunning);

            return pipeServer;
        }
    }
}