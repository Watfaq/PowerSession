using System;
using System.IO;
using System.Threading;
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

        public FileStream ConsoleOutStream { get; private set; }

        public event EventHandler OutputReady;

        public Terminal(bool enableAnsiEscape = true)
        {
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

        public void Start(string command, int consoleWidth = 80, int consoleHeight = 30)
        {
            using (var inputPipe = new PseudoConsolePipe())
            using (var outputPipe = new PseudoConsolePipe())
            using (var pseudoConsole = PseudoConsole.Create(inputPipe.ReadSide, outputPipe.WriteSide, consoleWidth, consoleHeight))
            using (var process = ProcessFactory.Start(command, PseudoConsole.PseudoConsoleThreadAttribute, pseudoConsole.Handle))
            {
                ConsoleOutStream = new FileStream(outputPipe.ReadSide, FileAccess.Read);
                OutputReady.Invoke(this, EventArgs.Empty);

                _consoleInputPipeWriteHandle = inputPipe.WriteSide;
                _consoleInputWriter = new StreamWriter(new FileStream(_consoleInputPipeWriteHandle, FileAccess.Write)){AutoFlush = true};
                
                OnClose(() => DisposeResources(process, pseudoConsole, outputPipe, inputPipe, _consoleInputWriter));
                WaitForExit(process).WaitOne(Timeout.Infinite);
            }
            
        }
        
        public void WriteToPseudoConsole(string input)
        {
            if (_consoleInputWriter == null)
            {
                throw new InvalidOperationException("There is no writer attached to a pseudoconsole. Have you called Start on this instance yet?");
            }
            _consoleInputWriter.Write(input);
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