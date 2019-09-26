using System;
using System.Runtime.InteropServices;

namespace PowerSession.ConPTY.Native
{
    static class ConsoleApi
    {    
        internal const int STD_OUTPUT_HANDLE = -11;
        internal const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        internal const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;
        
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        internal static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        internal static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        internal delegate bool ConsoleEventDelegate(CtrlTypes ctrl);
        
        internal enum CtrlTypes : uint
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }
    }
}