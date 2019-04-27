using System;
using System.Runtime.InteropServices;

namespace Simuro5v5
{
    static class Win32Dialog
    {
        public enum MessageBoxResult
        {
            Abort = 3,
            Cancel = 2,
            Continue = 11,
            Ignore = 5,
            No = 7,
            Ok = 1,
            Retry = 4,
            TryAgain = 10,
            Yes = 6,
        };

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        extern static int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        public static MessageBoxResult ShowMessageBox(string text, string caption)
        {
            return (MessageBoxResult)MessageBox(new IntPtr(0), text, caption, 0x00040000);
        }
    }
}
