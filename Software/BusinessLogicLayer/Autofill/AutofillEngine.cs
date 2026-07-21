using BusinessLogicLayer.AccountManagement;
using System;
using System.Runtime.InteropServices;
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
        private const uint VK_CONTROL = 0x11;
        private const uint VK_V = 0x56;

        private const uint WM_HOTKEY = 0x0312;
        private const uint WM_QUIT = 0x0012;
        private const int HWND_MESSAGE = -3;
        private const int GWLP_WNDPROC = -4;
        private const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
        private const uint WINEVENT_OUTOFCONTEXT = 0x0000;

        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        private const uint CF_UNICODETEXT = 13;
        private const uint GMEM_MOVEABLE = 0x0002;

        private readonly IAccountStore accountStore;
        private readonly ManualResetEventSlim windowReadySignal = new(false);
        private readonly WndProcDelegate wndProcDelegate;
        private readonly WinEventDelegate winEventDelegate;
        private IntPtr winEventHook;

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
            winEventDelegate = WinEventCallback;
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
            if (lastactiveWindowHandle == IntPtr.Zero || !IsWindow(lastactiveWindowHandle)) return false;

            var credentials = accountStore.GetCredentials(accountId);
            if (credentials == null) return false;

            SimulateInput(credentials.UserName, credentials.Password, lastactiveWindowHandle);

            return true;
        }

        public void UnregisterHotkeys()
        {
            if (!isHotKeyRegistered) return;

            UnregisterHotKey(windowHandle, HOTKEY_ID);
            UnhookWinEvent(winEventHook);
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

            winEventHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, winEventDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);

            while (GetMessageW(out var msg, IntPtr.Zero, 0, 0) > 0)
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

        private void WinEventCallback(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint eventThread, uint eventTime)
        {
            if (hwnd == IntPtr.Zero) return;

            GetWindowThreadProcessId(hwnd, out var processId);
            if (processId != GetCurrentProcessId())
            {
                lastactiveWindowHandle = hwnd;
            }
        }

        private static void SimulateInput(string username, string password, IntPtr targetWindowHandle)
        {
            SetForegroundWindow(targetWindowHandle);
            Thread.Sleep(150);

            PasteText(username);
            SendKeyPress(VK_TAB);
            Thread.Sleep(100);
            PasteText(password);

            SetClipboardText(string.Empty);
        }

        private static void PasteText(string text)
        {
            SetClipboardText(text);
            Thread.Sleep(50);
            SendPasteCombo();
            Thread.Sleep(50);
        }

        private static void SendPasteCombo()
        {
            var inputs = new[]
            {
                BuildKeyDown(VK_CONTROL),
                BuildKeyDown(VK_V),
                BuildKeyUp(VK_V),
                BuildKeyUp(VK_CONTROL)
            };

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private static void SendKeyPress(uint vk)
        {
            var inputs = new[] { BuildKeyDown(vk), BuildKeyUp(vk) };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private static INPUT BuildKeyDown(uint vk) => new INPUT
        {
            type = INPUT_KEYBOARD,
            U = new InputUnion { ki = new KEYBDINPUT { wVk = (ushort)vk, wScan = 0, dwFlags = 0, time = 0, dwExtraInfo = IntPtr.Zero } }
        };

        private static INPUT BuildKeyUp(uint vk) => new INPUT
        {
            type = INPUT_KEYBOARD,
            U = new InputUnion { ki = new KEYBDINPUT { wVk = (ushort)vk, wScan = 0, dwFlags = KEYEVENTF_KEYUP, time = 0, dwExtraInfo = IntPtr.Zero } }
        };

        private static void SetClipboardText(string text)
        {
            if (!OpenClipboard(IntPtr.Zero)) return;

            EmptyClipboard();

            var byteCount = (text.Length + 1) * 2;
            var hGlobal = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)byteCount);
            if (hGlobal != IntPtr.Zero)
            {
                var target = GlobalLock(hGlobal);
                if (target != IntPtr.Zero)
                {
                    var chars = text.ToCharArray();
                    Marshal.Copy(chars, 0, target, chars.Length);
                    Marshal.WriteInt16(target, chars.Length * 2, 0);
                    GlobalUnlock(hGlobal);

                    SetClipboardData(CF_UNICODETEXT, hGlobal);
                }
            }

            CloseClipboard();
        }

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint eventThread, uint eventTime);

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
        private static extern bool IsWindow(IntPtr hWnd);

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

        [DllImport("user32.dll")]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        private static extern bool EmptyClipboard();

        [DllImport("user32.dll")]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentProcessId();

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
