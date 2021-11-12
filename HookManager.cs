using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AJeHelperD2R
{
    class HookManager
    {
        private static HookManager sManager = new HookManager();
        public static HookManager getInstance() { return sManager; }

        private HookManager(){}

        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;

        public const int WM_KEYDOWN = 0x100;
        public const int WM_SYSKEYDOWN = 0x104;
        public const int WM_KEYUP = 0x101;
        public const int WM_SYSKEYUP = 0x105;

        public const int KEY_DOWN = 0x00;
        public const int KEY_EXTENDEDKEY = 0x01;
        public const int KEY_UP = 0x02;

        public const int VK_SHIFT = 0x10;
        public const int VK_CTRL = 0x11;
        public const int VK_MENU = 0x12;

        public const int WM_MOUSEHOVER = 0x02A1;
        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_RBUTTONUP = 0x0205;
        public const int WM_MBUTTONDOWN = 0x0207;
        public const int WM_MBUTTONUP = 0x0208;
        public const int WM_MWHEEL = 0x020A;
        public const int WM_XBUTTONDOWN = 0x020B;
        public const int WM_XBUTTONUP = 0x020C;

        public const int WM_XBUTTONDOWN2 = 0x020D;  // 임시로 (WM 메시지가 아님)
        public const int WM_XBUTTONUP2 = 0x020E;    // 임시로 (WM 메시지가 아님)

        public const int WM_MWHEELUP = 0x0210;    // 임시로 (WM 메시지가 아님)
        public const int WM_MWHEELDOWN = 0x0211;    // 임시로 (WM 메시지가 아님)

        public const int MOUSEMOVE = 0x00000001; // 마우스 이동
        public const int LB_DOWN = 0x00000002; // 왼쪽 마우스 버튼 눌림
        public const int LB_UP = 0x00000004; // 왼쪽 마우스 버튼 떼어짐
        public const int RB_DOWN = 0x00000008; // 오른쪽 마우스 버튼 눌림
        public const int RB_UP = 0x000000010; // 오른쪽 마우스 버튼 떼어짐
        public const int MB_DOWN = 0x00000020; // 휠 버튼 눌림
        public const int MB_UP = 0x000000040; // 휠 버튼 떼어짐
        public const int WHEEL = 0x00000800; //휠 스크롤
        public const int XB_DOWN = 0x0080;
        public const int XB_UP = 0x0100;
        public const int ABSOLUTEMOVE = 0x8000; // 전역 위치

        public const int XBUTTON1 = 0x0001;
        public const int XBUTTON2 = 0x0002;

        public const int NONE = 0;
        public const int EXTRA_INFO = 32;

        public delegate int OnHookHandler(int nCode, int wParam, IntPtr lParam);
        private static IntPtr sKeyHookInstance = IntPtr.Zero;
        private static IntPtr sMouseHookInstance = IntPtr.Zero;
        private static OnHookHandler sOnKeyHookHandler = onKeyHookHandler;
        private static OnHookHandler sOnKeyHookCallback = null;
        private static OnHookHandler sOnMouseHookHandler = onMouseHookHandler;
        private static OnHookHandler sOnMouseHookCallback = null;

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyHookStruct
        {
            public int vkCode;
            public int hardwareScanCode;
            public int flags;
            public int timeStamp;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStruct
        {
            public int x;
            public int y;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public bool startHook()
        {
            try
            {
                var hInstance = LoadLibrary("User32");
                if (hInstance == IntPtr.Zero)
                    return false;

                sKeyHookInstance = SetWindowsHookEx(WH_KEYBOARD_LL, sOnKeyHookHandler, hInstance, 0);
                if (sKeyHookInstance == IntPtr.Zero)
                    return false;

                sMouseHookInstance = SetWindowsHookEx(WH_MOUSE_LL, sOnMouseHookHandler, hInstance, 0);
                if (sMouseHookInstance == IntPtr.Zero)
                    return false;

                return true;
            }
            catch
            {
                sKeyHookInstance = IntPtr.Zero;
                sMouseHookInstance = IntPtr.Zero;
            }
            return false;
        }

        public void stopHook()
        {
            try
            {
                if (sKeyHookInstance != IntPtr.Zero)
                    UnhookWindowsHookEx(sKeyHookInstance);

                if (sMouseHookInstance != IntPtr.Zero)
                    UnhookWindowsHookEx(sMouseHookInstance);
            }
            catch { }
            sKeyHookInstance = IntPtr.Zero;
            sMouseHookInstance = IntPtr.Zero;
        }

        public void setKeyHookCallback(OnHookHandler onHookHandler)
        {
            sOnKeyHookCallback = onHookHandler;
        }

        public void unKeyHookCallback()
        {
            sOnKeyHookCallback = null;
        }

        public void setMouseHookCallback(OnHookHandler onHookHandler)
        {
            sOnMouseHookCallback = onHookHandler;
        }

        public void unMouseHookCallback()
        {
            sOnMouseHookCallback = null;
        }

        private static int onKeyHookHandler(int code, int wParam, IntPtr lParam)
        {
            if (sOnKeyHookCallback != null)
            {
                int value = sOnKeyHookCallback(code, wParam, lParam);
                if (value == 0)
                    return 0;
            }
            return CallNextHookEx(sKeyHookInstance, code, wParam, lParam);
        }

        private static int onMouseHookHandler(int code, int wParam, IntPtr lParam)
        {
            if (sOnMouseHookCallback != null)
            {
                int value = sOnMouseHookCallback(code, wParam, lParam);
                if (value == 0)
                    return 0;
            }
            return CallNextHookEx(sMouseHookInstance, code, wParam, lParam);
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int hookID, OnHookHandler handler, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(IntPtr hookID);

        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(IntPtr hookID, int nCode, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, IntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern void keybd_event(uint vkCode, uint scan, uint flags, IntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(int vkCode, int mapType);

        [DllImport("user32.dll")]
        public static extern int GetCursorPos(ref Point lpPoint);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        public static string getCaptionOfActiveWindow()
        {
            var strTitle = string.Empty;
            var handle = GetForegroundWindow();
            // Obtain the length of the text   
            var intLength = GetWindowTextLength(handle) + 1;
            var stringBuilder = new StringBuilder(intLength);
            if (GetWindowText(handle, stringBuilder, intLength) > 0)
            {
                strTitle = stringBuilder.ToString();
            }
            return strTitle;
        }
    }
}
