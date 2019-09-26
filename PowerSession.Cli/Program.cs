using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using PowerSession.ConPTY;

namespace PowerSession.Cli
{
    class Program
    {
        private static Terminal _terminal;
        
        static void Main(string[] args)
        {
            _terminal = new Terminal();
            _terminal.OutputReady += Terminal_OutputReady;
            Task.Run(() => _terminal.Start("powershell.exe", Console.WindowWidth, Console.WindowHeight));
            Console.WriteLine("Type exit to stop");
            do
            {
                var key = Console.ReadKey(true);
                _terminal.WriteToPseudoConsole(key.KeyChar.ToString());
            } while (true);
        }

        private static void Terminal_OutputReady(object sender, EventArgs e)
        {
            Task.Factory.StartNew(CopyConsoleToWindow, TaskCreationOptions.LongRunning);
        }

        private static void CopyConsoleToWindow()
        {
            using var reader = new StreamReader(_terminal.ConsoleOutStream);
            char[] buf = new char[1];
            while ((reader.ReadBlock(buf, 0, 1)) != 0)
            {
                Console.Out.Write(buf);
            }
        }
    }
}