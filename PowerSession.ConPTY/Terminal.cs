using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using PowerSession.ConPTY.Processes;
using static PowerSession.ConPTY.Native.ConsoleApi;

namespace PowerSession.ConPTY
{
    public sealed class Terminal
    {
        private const string ExitCommand = "exit\r";
        private const string CtrlC_Command = "\x3";
        
        private SafeFileHandle _consoleInputPipeWriteHandle;
        private StreamWriter _consoleInputWriter;
        private FileStream _consoleOutStream;

        private readonly Stream _inputReader;
        private readonly Stream _outputWriter;

        public Terminal(Stream inputReader = null, Stream outputWriter = null, bool enableAnsiEscape = true)
        {
            _inputReader = inputReader;
            _outputWriter = outputWriter;
            
            if (enableAnsiEscape)
            {
                var stdout = GetStdHandle(STD_OUTPUT_HANDLE);
                if (GetConsoleMode(stdout, out var consoleMode))
                {
                    consoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;
                    SetConsoleMode(stdout, consoleMode);
                }
            }
        }

        public void Record(string command, IDictionary environment = null)
        {
            var (width, height) = (Console.WindowWidth, Console.WindowHeight);
    
            using var inputPipe = new PseudoConsolePipe();
            using var outputPipe = new PseudoConsolePipe();
            using var pseudoConsole = PseudoConsole.Create(inputPipe.ReadSide, outputPipe.WriteSide, width, height);
            using var process = ProcessFactory.Start(command, PseudoConsole.PseudoConsoleThreadAttribute, pseudoConsole.Handle);
            _consoleOutStream = new FileStream(outputPipe.ReadSide, FileAccess.Read);

            _consoleInputPipeWriteHandle = inputPipe.WriteSide;
            _consoleInputWriter = new StreamWriter(new FileStream(_consoleInputPipeWriteHandle, FileAccess.Write)){AutoFlush = true};
            
            AttachStdin();
            ConnectOutput(_outputWriter);
                
            OnClose(() => DisposeResources(process, pseudoConsole, outputPipe, inputPipe, _consoleInputWriter));
            WaitForExit(process).WaitOne(Timeout.Infinite);
        }

        private void AttachStdin()
        {
            Task.Factory.StartNew(() =>
            {
                ConsoleKeyInfo key;
                while ((key = Console.ReadKey(true)).Key != ConsoleKey.Escape)
                {
                    _consoleInputWriter.Write(key.KeyChar);
                }
            }, TaskCreationOptions.LongRunning);
        }
        

        private void ConnectOutput(Stream outputStream)
        {
            if (_outputWriter == null) return;

            Task.Factory.StartNew(() =>
            {
                _consoleOutStream.CopyTo(outputStream);
            }, TaskCreationOptions.LongRunning);
        }
        
        private static AutoResetEvent WaitForExit(Process process) =>
            new AutoResetEvent(false)
            {
                SafeWaitHandle = new SafeWaitHandle(process.ProcessInfo.hProcess, ownsHandle: false)
            };
        
        private static void OnClose(Action handler)
        {
            SetConsoleCtrlHandler(eventType =>
            {
                if (eventType == CtrlTypes.CTRL_CLOSE_EVENT)
                {
                    handler();
                }
                return false;
            }, true);
        }
        
        private void DisposeResources(params IDisposable[] disposables)
        {
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
        }
    }
}