using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;

namespace PowerSession.Commands
{
    
    public class FileWriter : IDisposable
    {
        private FileStream _fileStream;
        private DateTimeOffset _startTimeStamp;
        
        public FileWriter(string output)
        {
            if (!File.Exists(output)) throw new FileNotFoundException(output);
            _fileStream = File.OpenWrite(output);
            _startTimeStamp = DateTimeOffset.Now;
        }

        /// <summary>
        /// Record stdin
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
                using var sw = new StreamWriter(_fileStream);
                var buf = new char[1024];
                int bytesRead;
                while ((bytesRead = sr.Read(buf)) != 0)
                {
                    var ts = DateTimeOffset.Now - _startTimeStamp;
                    sw.WriteLine($"[{ts.TotalMilliseconds}, \"o\", {buf}]");  // asciinema compatible
                    Console.Out.Write(buf.Take(bytesRead).ToArray());
                }
            }, TaskCreationOptions.LongRunning);
            
            return pipeServer;
        }

        public void Dispose()
        {
            _fileStream?.Dispose();
        }
    }
}