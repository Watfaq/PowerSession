using static PowerSession.Main.ConPTY.Native.ProcessApi;

namespace PowerSession.Main.ConPTY.Processes
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using Native;

    internal static class ProcessFactory
    {
        /// <summary>
        ///     Start and configure a process. The return value represents the process and should be disposed.
        /// </summary>
        internal static Process Start(string command, IntPtr attributes, IntPtr hPC)
        {
            var startupInfo = ConfigureProcessThread(hPC, attributes);
            var processInfo = RunProcess(ref startupInfo, command);
            return new Process(startupInfo, processInfo);
        }

        private static ProcessApi.STARTUPINFOEX ConfigureProcessThread(IntPtr hPC, IntPtr attributes)
        {
            // this method implements the behavior described in https://docs.microsoft.com/en-us/windows/console/creating-a-pseudoconsole-session#preparing-for-creation-of-the-child-process

            var lpSize = IntPtr.Zero;
            var success = InitializeProcThreadAttributeList(
                IntPtr.Zero,
                1,
                0,
                ref lpSize
            );
            if (success || lpSize == IntPtr.Zero
            ) // we're not expecting `success` here, we just want to get the calculated lpSize
                throw new Win32Exception(Marshal.GetLastWin32Error(),
                    "Could not calculate the number of bytes for the attribute list.");

            var startupInfo = new ProcessApi.STARTUPINFOEX();
            startupInfo.StartupInfo.cb = Marshal.SizeOf<ProcessApi.STARTUPINFOEX>();
            startupInfo.lpAttributeList = Marshal.AllocHGlobal(lpSize);

            success = InitializeProcThreadAttributeList(
                startupInfo.lpAttributeList,
                1,
                0,
                ref lpSize
            );
            if (!success) throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not set up attribute list.");

            success = UpdateProcThreadAttribute(
                startupInfo.lpAttributeList,
                0,
                attributes,
                hPC,
                (IntPtr) IntPtr.Size,
                IntPtr.Zero,
                IntPtr.Zero
            );
            if (!success)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not set pseudoconsole thread attribute.");

            return startupInfo;
        }

        private static ProcessApi.PROCESS_INFORMATION RunProcess(ref ProcessApi.STARTUPINFOEX sInfoEx, string commandLine)
        {
            var securityAttributeSize = Marshal.SizeOf<ProcessApi.SECURITY_ATTRIBUTES>();
            var pSec = new ProcessApi.SECURITY_ATTRIBUTES {nLength = securityAttributeSize};
            var tSec = new ProcessApi.SECURITY_ATTRIBUTES {nLength = securityAttributeSize};
            var success = CreateProcess(
                null,
                commandLine,
                ref pSec,
                ref tSec,
                false,
                EXTENDED_STARTUPINFO_PRESENT,
                IntPtr.Zero,
                null,
                ref sInfoEx,
                out var pInfo
            );
            if (!success) throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not create process.");

            return pInfo;
        }
    }
}