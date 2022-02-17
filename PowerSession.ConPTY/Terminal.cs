using static PowerSession.Main.ConPTY.Native.ConsoleApi;

namespace PowerSession.Main.ConPTY
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Win32.SafeHandles;
    using Native;
    using Processes;

    public sealed class Terminal
    {
        private const string CtrlC_Command = "\x3";
        private char[] UpArrow = new []{(char) 0x1b, (char) 0x5b, 'A'};
        private char[] DownArrow = new []{(char) 0x1b, (char) 0x5b, 'B'};
        private char[] RightArrow = new []{(char) 0x1b, (char) 0x5b, 'C'};
        private char[] LeftArrow = new []{(char) 0x1b, (char) 0x5b, 'D'};

        private readonly Stream _inputReader;
        private readonly Stream _outputWriter;
        private readonly int _height;
        private readonly int _width;

        private SafeFileHandle _consoleInputPipeWriteHandle;
        private StreamWriter _consoleInputWriter;
        private FileStream _consoleOutStream;

        private readonly CancellationTokenSource _tokenSource;
        private readonly CancellationToken _token;

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
                    if (!SetConsoleMode(stdout, consoleMode))
                    {
                        throw new NotSupportedException("VIRTUAL_TERMINAL_PROCESSING");
                    }
                }
            }

            _width = width == 0 ? Console.WindowWidth : width;
            _height = height == 0 ? Console.WindowHeight : height;

            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
        }

        public void Record(string command, IDictionary environment = null)
        {
            using var inputPipe = new PseudoConsolePipe();
            using var outputPipe = new PseudoConsolePipe();
            using var pseudoConsole = PseudoConsole.Create(inputPipe.ReadSide, outputPipe.WriteSide, _width, _height);

            using var process = ProcessFactory.Start($"powershell.exe -c \"Set-Item -Path Env:POWERSESSION_RECORDING -Value 1;{command}\"", PseudoConsole.PseudoConsoleThreadAttribute,
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

            _tokenSource.Cancel();
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
                while (!_token.IsCancellationRequested)
                {
                    var key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.UpArrow:
                            _consoleInputWriter.Write(UpArrow);
                            break;
                        case ConsoleKey.DownArrow:
                            _consoleInputWriter.Write(DownArrow);
                            break;
                        case ConsoleKey.RightArrow:
                            _consoleInputWriter.Write(RightArrow);
                            break;
                        case ConsoleKey.LeftArrow:
                            _consoleInputWriter.Write(LeftArrow);
                            break;
                        default:
                            _consoleInputWriter.Write(key.KeyChar);
                            break;
                    }
                }
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
                if (eventType == ConsoleApi.CtrlTypes.CTRL_CLOSE_EVENT) handler();
                return false;
            }, true);
        }

        private static void DisposeResources(params IDisposable[] disposables)
        {
            foreach (var disposable in disposables) disposable?.Dispose();
        }
    }
}