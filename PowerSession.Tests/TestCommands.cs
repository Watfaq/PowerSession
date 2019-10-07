using NUnit.Framework;

namespace PowerSession.Main.Commands.Tests
{
    using System;
    using System.IO;
    using System.IO.Pipes;
    using System.Threading;
    using System.Threading.Tasks;

    public class CommandsTests
    {
        private RecordCommand _recordCommand;
        private string _filePath;
        private StreamWriter _stdinWriter;
        
        [SetUp]
        public void Setup()
        {
            var pipeServer = new AnonymousPipeServerStream();
            var pipeClient = new AnonymousPipeClientStream(pipeServer.GetClientHandleAsString());
            Console.SetIn(new StreamReader(pipeClient));
            _stdinWriter = new StreamWriter(pipeServer);
            
            var tempFile = Path.GetTempFileName();
            var recordArgs = new RecordArgs
            {
                Filename = tempFile
            };
            _recordCommand = new RecordCommand(recordArgs);
            _filePath = tempFile;

        }

        [Test]
        [Ignore("TODO")]
        public void TestExecute()
        {
            var exitSend = new AutoResetEvent(false);
            Task.Factory.StartNew(() =>
            {
                _recordCommand.Execute();
                exitSend.Set();
            });
            
            _stdinWriter.Write("\x3");
            exitSend.WaitOne();
            StringAssert.Contains("PowerShell", File.ReadAllText(_filePath), "Record result should contain keyword");
        }
    }
}