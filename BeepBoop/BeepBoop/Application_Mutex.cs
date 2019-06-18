using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace BeepBoop
{
    public class Application_Mutex
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// Restore the window in ShowWindow().
        /// </summary>
        private const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// Check if this process is the first of its kind, otherwise open the first process to the foreground.
        /// </summary>
        /// <returns>True on first process.</returns>
        public static bool Mutex()
        {
            #region Only allow one instance of this application
            Process[] processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            if (processes.Length > 1)
            {
                //Get the first process (not this process).
                Process first = processes.First(p => p != Process.GetCurrentProcess());

                //Restore to normal size.
                ShowWindow(first.MainWindowHandle, SW_RESTORE);

                //Move to foreground.
                SetForegroundWindow(first.MainWindowHandle);

                //No new process started.
                return false;
            }
            #endregion

            //New process started.
            return true;
        }
    }
}
