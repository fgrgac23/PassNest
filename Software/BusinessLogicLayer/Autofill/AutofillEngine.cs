using BusinessLogicLayer.AccountManagement;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;

namespace BusinessLogicLayer.Autofill
{
    public class AutofillEngine : IAutofillEngine
    {
        private const int HOTKEY_ID = 1;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_ALT = 0x0001;
        private const uint VK_P = 0x50;
        private const uint VK_TAB = 0x09;

        private const uint WM_HOTKEY = 0x0312;
        private const uint WM_QUIT = 0x0012;
        private const int HWND_MESSAGE = -3;
        private const int GWLP_WNDPROC = -4;

        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_UNICODE = 0x0004;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        private readonly IAccountStore accountStore;
        private readonly ManualResetEventSlim windowReadySignal = new(false);
        private readonly WndProcDelegate wndProcDelegate;

        private bool isHotKeyRegistered;
        private IntPtr lastactiveWindowHandle;
        private IntPtr windowHandle;
        private uint messageLoopThreadId;
        private Thread? messageLoopThread;

        public event Action? HotkeyPressed;

        public AutofillEngine(IAccountStore accountStore)
        {
            this.accountStore = accountStore;
            wndProcDelegate = WndProc;
        }
        public void RegisterHotkeys()
        {
            if (isHotKeyRegistered) return;

            messageLoopThread = new Thread(RunMessageLoop)
            {
                IsBackground = true
            };
            messageLoopThread.SetApartmentState(ApartmentState.STA);
            messageLoopThread.Start();

            windowReadySignal.Wait();
            isHotKeyRegistered = true;
        }

        public bool TriggerAutofill(int accountId)
        {
            var credentials = accountStore.GetCredentials(accountId);
            if (credentials == null) return false;

            SimulateInput(credentials.UserName, credentials.Password, lastactiveWindowHandle);

            return true;
        }

        public void UnregisterHotkeys()
        {
            if (!isHotKeyRegistered) return;

            UnregisterHotKey(windowHandle, HOTKEY_ID);
            DestroyWindow(windowHandle);
            PostThreadMessage(messageLoopThreadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
            messageLoopThread?.Join(500);

            windowHandle = IntPtr.Zero;
            windowReadySignal.Reset();
            isHotKeyRegistered = false;
        }

        private void RunMessageLoop()
        {
            messageLoopThreadId = GetCurrentThreadId();

            windowHandle = CreateWindowExW(0, "STATIC", "PassNestAutofillHotkeyWindow", 0, 0, 0, 0, 0, new IntPtr(HWND_MESSAGE), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            SetWindowLongPtr(windowHandle, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(wndProcDelegate));

            RegisterHotKey(windowHandle, HOTKEY_ID, MOD_CONTROL | MOD_ALT, VK_P);

            windowReadySignal.Set();

            while(GetMessageW(out var msg, IntPtr.Zero, 0, 0) > 0)
            {
                TranslateMessage(ref msg);
                DispatchMessageW(ref msg);
            }
        }

        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if(msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                lastactiveWindowHandle = GetForegroundWindow();
                HotkeyPressed?.Invoke();
                return IntPtr.Zero;
            }

            return DefWindowProcW(hWnd, msg, wParam, lParam);
        }

        private static void SimulateInput(string username, string password, IntPtr targetWindowHandle)
        {
            SetForegroundWindow(targetWindowHandle);
            Thread.Sleep(150);

            var inputs = new List<INPUT>();
            foreach (var c in username) inputs.AddRange(BuildUnicodeCharInputs(c));
            inputs.AddRange(BuildKeyInputs(VK_TAB));
            foreach (var c in password) inputs.AddRange(BuildUnicodeCharInputs(c));

            var inputArray = inputs.ToArray();
            SendInput((uint)inputArray.Length, inputArray, Marshal.SizeOf(typeof(INPUT)));
        }

        private static INPUT[] BuildUnicodeCharInputs(char c) => new[]
        {
            new INPUT
            {
                type = INPUT_KEYBOARD, U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0, wScan = (ushort)c, dwFlags = KEYEVENTF_UNICODE, time = 0, dwExtraInfo = IntPtr.Zero
                    }
                }
            },
            new INPUT
            {
                type = INPUT_KEYBOARD, U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0, wScan = (ushort)c, dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP, time = 0, dwExtraInfo = IntPtr.Zero
                    }
                }
            },
        };

        private static INPUT[] BuildKeyInputs(uint vk) => new[]
        {
            new INPUT
            {
                type = INPUT_KEYBOARD, U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = (ushort)vk, wScan = 0, dwFlags = 0, time = 0, dwExtraInfo = IntPtr.Zero
                    }
                }
            },
            new INPUT
            {
                type = INPUT_KEYBOARD, U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = (ushort)vk, wScan = 0, dwFlags = KEYEVENTF_KEYUP, time = 0, dwExtraInfo = IntPtr.Zero
                    }
                }
            },
        };

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public int ptX;
            public int ptY;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
            [FieldOffset(0)] public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion U;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetMessageW(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessageW(ref MSG lpMsg);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateWindowExW(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool PostThreadMessage(uint idThread, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            return IntPtr.Size == 8 ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong) : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    }
}
