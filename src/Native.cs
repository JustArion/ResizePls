namespace Dawn.Resize
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;

    public abstract class Native
    {
        public static readonly HashSet<IntPtr> CachedWindowPointers = new();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText ( IntPtr hWnd, [Out] StringBuilder lpString, int nMaxCount);
        public static string GetWindowText(IntPtr ptr)
        {
            var builder = new StringBuilder(150);
            GetWindowText(ptr, builder, builder.Capacity);
            return builder.ToString();
        }
        
        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr parentWindow, IntPtr previousChildWindow, string windowClass, string windowTitle);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr window, out int process);
        
        internal static IntPtr[] GetProcessWindows(int process) {
            var apRet = new IntPtr[256];
            var iCount = 0;
            var pLast = IntPtr.Zero;
            do {
                pLast = FindWindowEx(IntPtr.Zero, pLast, null, null);
                GetWindowThreadProcessId(pLast, out var iProcess_);
                if(iProcess_ == process) apRet[iCount++] = pLast;
            } while(pLast != IntPtr.Zero);
            Array.Resize(ref apRet, iCount);
            return apRet;
        }
    }
}