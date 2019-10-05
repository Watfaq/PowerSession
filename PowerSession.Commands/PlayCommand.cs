using static PowerSession.ConPTY.Native.ConsoleApi;

namespace PowerSession.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public struct PlayArgs
    {
        public string Filename;
        public bool EnableAnsiEscape;
    }

    public class PlayCommand : ICommand
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly AutoResetEvent _dataReceivedEvent;
        private readonly AutoResetEvent _readEvent;

        private readonly CancellationToken _readlineCancellationToken;
        private readonly RecordSession _session;
        private ConsoleKeyInfo _consoleKeyInfo;

        public PlayCommand(PlayArgs args)
        {
            _session = new RecordSession(args.Filename);

            _cancellationTokenSource = new CancellationTokenSource();
            _readlineCancellationToken = _cancellationTokenSource.Token;
            _readEvent = new AutoResetEvent(false);
            _dataReceivedEvent = new AutoResetEvent(false);
            Task.Factory.StartNew(ReadKey, TaskCreationOptions.LongRunning);

            if (args.EnableAnsiEscape)
            {
                var stdout = GetStdHandle(STD_OUTPUT_HANDLE);
                if (GetConsoleMode(stdout, out var consoleMode))
                {
                    consoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;
                    if (!SetConsoleMode(stdout, consoleMode))
                        throw new NotSupportedException("VIRTUAL_TERMINAL_PROCESSING");
                }
            }
        }

        public int Execute()
        {
            foreach (var line in _session.StdoutRelativeTime())
            {
                _readEvent.Set();
                var dataReceived = _dataReceivedEvent.WaitOne(TimeSpan.FromSeconds(line.Timestamp));
                if (dataReceived)
                    switch (_consoleKeyInfo.KeyChar)
                    {
                        case '\x3': //Control C
                            _cancellationTokenSource.Cancel();
                            break;
                    }

                Console.Out.Write(line.Content);
            }

            return 0;
        }

        private void ReadKey()
        {
            while (!_readlineCancellationToken.IsCancellationRequested)
            {
                _readEvent.WaitOne();
                _consoleKeyInfo = Console.ReadKey();
                _dataReceivedEvent.Set();
            }
        }
    }
}