using System;
using System.ComponentModel;
using Microsoft.Win32.SafeHandles;
using static PowerSession.ConPTY.Native.PseudoConsoleApi;
using static PowerSession.ConPTY.Native.ConsoleApi;

namespace PowerSession.ConPTY
{
    internal sealed class PseudoConsole : IDisposable
    {
        public static readonly IntPtr PseudoConsoleThreadAttribute = (IntPtr) PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE;
        public IntPtr Handle { get; }

        private PseudoConsole(IntPtr handle)
        {
            Handle = handle;
        }

        internal static PseudoConsole Create(SafeFileHandle inputReadSide, SafeFileHandle outputWriteSide, int width,
            int height)
        {
            var createResult = CreatePseudoConsole(
                new COORD {X = (short) width, Y = (short) height},
                inputReadSide, outputWriteSide,
                0, out IntPtr hPC);
            if (createResult != 0)
            {
                throw new Win32Exception(createResult, "Failed to create pseudo console.");
            }
            return new PseudoConsole(hPC);
        }

        public void Dispose()
        {
            ClosePseudoConsole(Handle);
        }
    }
}