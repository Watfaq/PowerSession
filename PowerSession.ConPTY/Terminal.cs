using static PowerSession.ConPTY.Native.ConsoleApi;

namespace PowerSession.ConPTY
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Win32.SafeHandles;
    using Processes;

    public sealed class Terminal
    {
        private const string ExitCommand = "exit\r";
        private const string CtrlC_Command = "\x3";

        private readonly Stream _inputReader;
        private readonly Stream _outputWriter;
        public readonly int Height;

        public readonly int Width;

        private SafeFileHandle _consoleInputPipeWriteHandle;
        private StreamWriter _consoleInputWriter;
        private FileStream _consoleOutStream;

        public Terminal(Stream inputReader = null, Stream outputWriter = null, bool enableAnsiEscape = true,
            int width = default, int height = default)
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

            Width = width == 0 ? Console.WindowWidth : width;
            Height = height == 0 ? Console.WindowHeight : height;
        }

        public void Record(string command, IDictionary environment = null)
        {
            using var inputPipe = new PseudoConsolePipe();
            using var outputPipe = new PseudoConsolePipe();
            using var pseudoConsole = PseudoConsole.Create(inputPipe.ReadSide, outputPipe.WriteSide, Width, Height);
            using var process = ProcessFactory.Start(command, PseudoConsole.PseudoConsoleThreadAttribute,
                pseudoConsole.Handle);
            _consoleOutStream = new FileStream(outputPipe.ReadSide, FileAccess.Read);

            _consoleInputPipeWriteHandle = inputPipe.WriteSide;
            _consoleInputWriter = new StreamWriter(new FileStream(_consoleInputPipeWriteHandle, FileAccess.Write))
                {AutoFlush = true};

            AttachStdin();
            ConnectOutput(_outputWriter);

            OnClose(() => DisposeResources(process, pseudoConsole, outputPipe, inputPipe, _consoleInputWriter,
                _consoleOutStream));
            WaitForExit(process).WaitOne(Timeout.Infinite);

            _consoleOutStream.Close();
            _consoleInputWriter.Close();
            _outputWriter.Close();
        }

        private void AttachStdin()
        {
            Console.CancelKeyPress += (sender, args) =>
            {
                args.Cancel = true;
                _consoleInputWriter.Write(CtrlC_Command);
            };

            Task.Factory.StartNew(() =>
            {
                ConsoleKeyInfo key;
                while ((key = Console.ReadKey(true)).Key != ConsoleKey.Escape) _consoleInputWriter.Write(key.KeyChar);
            }, TaskCreationOptions.LongRunning);
        }


        private void ConnectOutput(Stream outputStream)
        {
            if (_outputWriter == null) return;

            Task.Factory.StartNew(() => { _consoleOutStream.CopyTo(outputStream); }, TaskCreationOptions.LongRunning);
        }

        private static AutoResetEvent WaitForExit(Process process)
        {
            return new AutoResetEvent(false)
            {
                SafeWaitHandle = new SafeWaitHandle(process.ProcessInfo.hProcess, false)
            };
        }

        private static void OnClose(Action handler)
        {
            SetConsoleCtrlHandler(eventType =>
            {
                if (eventType == CtrlTypes.CTRL_CLOSE_EVENT) handler();
                return false;
            }, true);
        }

        private static void DisposeResources(params IDisposable[] disposables)
        {
            foreach (var disposable in disposables) disposable?.Dispose();
        }
    }
}