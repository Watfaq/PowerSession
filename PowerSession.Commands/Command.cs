using System;
using System.IO;

namespace PowerSession.Commands
{
    public interface ICommand
    {
        int Execute();
    }
    
    internal abstract class Command
    {
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