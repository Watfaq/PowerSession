namespace PowerSession.Commands
{
    using System;
    using System.IO;
    using Api;
    using Main.Api;
    using static Main.ConPTY.Native.ConsoleApi;

    public interface ICommand
    {
        int Execute();
    }

    public abstract class BaseCommand
    {
        protected static IApiService Api;

        protected BaseCommand()
        {
            Api = new AsciinemaApi();
        }
        
        protected BaseCommand(bool enableAnsiEscape) : this()
        {
            if (enableAnsiEscape)
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

        protected void Log(string text, TextWriter output = null)
        {
            if (output == null) output = Console.Out;
            output.WriteLine(text);
        }

        protected void LogInfo(string text)
        {
            Log($"\x1b[0;32mPowerSession: {text}\x1b[0m");
        }

        protected void LogWarning(string text)
        {
            Log($"\x1b[0;33mPowerSession: {text}\x1b[0m");
        }

        protected void LogError(string text)
        {
            Log($"\x1b[0;33mPowerSession: {text}\x1b[0m");
        }
    }
}