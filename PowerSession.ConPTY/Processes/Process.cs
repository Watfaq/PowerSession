using System;
using System.Runtime.InteropServices;
using PowerSession.ConPTY.Native;
using static PowerSession.ConPTY.Native.ProcessApi;

namespace PowerSession.ConPTY.Processes
{
    internal sealed class Process : IDisposable
    {
        public Process(ProcessApi.STARTUPINFOEX startupInfo, ProcessApi.PROCESS_INFORMATION processInfo)
        {
            StartupInfo = startupInfo;
            ProcessInfo = processInfo;
        }

        public ProcessApi.STARTUPINFOEX StartupInfo { get; }
        public ProcessApi.PROCESS_INFORMATION ProcessInfo { get; }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                }

                // dispose unmanaged state

                // Free the attribute list
                if (StartupInfo.lpAttributeList != IntPtr.Zero)
                {
                    DeleteProcThreadAttributeList(StartupInfo.lpAttributeList);
                    Marshal.FreeHGlobal(StartupInfo.lpAttributeList);
                }

                // Close process and thread handles
                if (ProcessInfo.hProcess != IntPtr.Zero)
                {
                    CloseHandle(ProcessInfo.hProcess);
                }
                if (ProcessInfo.hThread != IntPtr.Zero)
                {
                    CloseHandle(ProcessInfo.hThread);
                }

                disposedValue = true;
            }
        }

        ~Process()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}