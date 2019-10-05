namespace PowerSession.ConPTY.Native
{
    using System;
    using System.Runtime.InteropServices;

    public static class ConsoleApi
    {
        public const int STD_OUTPUT_HANDLE = -11;
        public const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        public const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

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