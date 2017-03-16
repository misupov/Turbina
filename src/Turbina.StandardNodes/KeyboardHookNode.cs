using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Turbina.Engine;

namespace Turbina.StandardNodes
{
    public interface IKeyboardHookNode : INode
    {
        IObserver<bool> IsEnabled { get; }
        IObservable<char> Key { get; }
        IObservable<int> Code { get; }
    }

    public sealed class KeyboardHookNode : Node, IKeyboardHookNode
    {
        private IntPtr _hook;
        private Thread _thread;
        public Inlet<bool> IsEnabled { get; set; }
        public Outlet<char> Key { get; set; }
        public Outlet<int> Code { get; set; }

        IObserver<bool> IKeyboardHookNode.IsEnabled => IsEnabled;
        IObservable<char> IKeyboardHookNode.Key => Key;
        IObservable<int> IKeyboardHookNode.Code => Code;

        public KeyboardHookNode(Workspace workspace) : base(workspace)
        {
        }

        protected override Task Initialize()
        {
            _thread = new Thread(o =>
            {
                using (var curProcess = Process.GetCurrentProcess())
                {
                    var moduleHandle = GetModuleHandle(curProcess.MainModule.ModuleName);
                    _hook = SetWindowsHookEx(WH_KEYBOARD_LL, Lpfn, moduleHandle, 0);
                }

                while (!GetMessage(out MSG msg, IntPtr.Zero, 0, 0))
                {
                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);
                }
            });
            _thread.Start();

            return Task.CompletedTask;
        }

        protected override Task Destroy()
        {
            UnhookWindowsHookEx(_hook);

            return Task.CompletedTask;
        }

        protected override Task Operate()
        {
            return Task.CompletedTask;
        }

        private int Lpfn(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code >= 0 && (wParam == (IntPtr) WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Code.Send(vkCode);

                if (wParam == (IntPtr) WM_KEYDOWN)
                {
                    int nonVirtualKey = MapVirtualKey((uint) vkCode, 2);

                    char mappedChar = Convert.ToChar(nonVirtualKey);
                    Key.Send(mappedChar);
                }

                //                if (((Keys)vkCode == Keys.LWin) || ((Keys)vkCode == Keys.RWin))
                //                {
                //                    Console.WriteLine("{0} blocked!", (Keys)vkCode);
                //                    return (IntPtr)1;
                //                }
            }
            return CallNextHookEx(_hook, code, wParam, lParam);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SetWindowsHookEx", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

        [DllImport("user32.dll")]
        private static extern int MapVirtualKey(uint uCode, uint uMapType);

        private delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);

        private const int WH_KEYBOARD = 2;

        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        private struct MSG
        {
            public IntPtr hwnd;

            public IntPtr lParam;

            public int message;

            public int pt_x;

            public int pt_y;

            public int time;

            public IntPtr wParam;
        }
    }
}