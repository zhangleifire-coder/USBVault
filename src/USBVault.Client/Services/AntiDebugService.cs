using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace USBVault.Client.Services
{
    /// <summary>
    /// Provides anti-debugging detection capabilities using both managed and native APIs.
    /// </summary>
    public static class AntiDebugService
    {
        private const int ProcessDebugPort = 7;

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(
            IntPtr process,
            int infoClass,
            IntPtr buffer,
            int size,
            out int returnLength);

        /// <summary>
        /// Returns true if a debugger is detected via either managed or native check.
        /// </summary>
        public static bool IsDebuggerAttached()
        {
            // Managed check
            if (Debugger.IsAttached)
                return true;

            // Native check via ntdll
            return IsDebuggerAttachedNative();
        }

        private static bool IsDebuggerAttachedNative()
        {
            IntPtr buffer = IntPtr.Zero;
            try
            {
                int returnLength;
                buffer = Marshal.AllocHGlobal(sizeof(int));
                int result = NtQueryInformationProcess(
                    Process.GetCurrentProcess().Handle,
                    ProcessDebugPort,
                    buffer,
                    sizeof(int),
                    out returnLength);

                // result == 0 (STATUS_SUCCESS) with a non-zero port value means debugger is attached
                if (result == 0)
                {
                    int portValue = Marshal.ReadInt32(buffer);
                    return portValue != 0;
                }
                return false;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                    Marshal.FreeHGlobal(buffer);
            }
        }

        /// <summary>
        /// Terminates the process immediately if a debugger is detected.
        /// </summary>
        public static void TerminateIfDebugging()
        {
            if (IsDebuggerAttached())
                Environment.Exit(1);
        }
    }
}