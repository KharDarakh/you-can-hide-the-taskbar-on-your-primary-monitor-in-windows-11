using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using YCHTTOPYMIW11.Core.Interop;

namespace YCHTTOPYMIW11.Core.Services
{
    public sealed class ShellRecoveryService : IShellRecoveryService
    {
        private readonly ILogger<ShellRecoveryService> _logger;
        private Thread? _messageThread;
        private IntPtr _hwndMessage;
        private uint _msgTaskbarCreated;
        private bool _isRunning;
        private bool _disposed;

        public event EventHandler? ExplorerRestarted;
        public event EventHandler? DisplayTopologyChanged;

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private WndProcDelegate? _wndProc;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct WNDCLASSEX
        {
            public int cbSize;
            public int style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string? lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateWindowEx(
            uint dwExStyle, string lpClassName, string lpWindowName,
            uint dwStyle, int x, int y, int nWidth, int nHeight,
            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern sbyte GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern bool PostThreadMessage(int idThread, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern int GetCurrentThreadId();

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        private const uint WM_QUIT = 0x0012;
        private static readonly IntPtr HWND_MESSAGE = new IntPtr(-3);

        public ShellRecoveryService(ILogger<ShellRecoveryService> logger)
        {
            _logger = logger;
        }

        public void StartListening()
        {
            if (_isRunning) return;
            _isRunning = true;

            var readyEvent = new ManualResetEventSlim(false);

            _messageThread = new Thread(() =>
            {
                DesktopSessionHelper.RunOnInteractiveDesktop(() =>
                {
                    _msgTaskbarCreated = NativeMethods.RegisterWindowMessage("TaskbarCreated");
                    _logger.LogInformation("Registered 'TaskbarCreated' message ID: 0x{Id:X}", _msgTaskbarCreated);

                    string className = "YCHTTOPYMIW11_MsgListener_" + Guid.NewGuid().ToString("N");
                    _wndProc = CustomWndProc;

                    var wcx = new WNDCLASSEX
                    {
                        cbSize = Marshal.SizeOf<WNDCLASSEX>(),
                        lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProc),
                        hInstance = IntPtr.Zero,
                        lpszClassName = className
                    };

                    RegisterClassEx(ref wcx);

                    _hwndMessage = CreateWindowEx(
                        0, className, "YCHTTOPYMIW11MessageWindow",
                        0, 0, 0, 0, 0,
                        HWND_MESSAGE, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

                    readyEvent.Set();

                    while (GetMessage(out MSG msg, IntPtr.Zero, 0, 0) > 0)
                    {
                        TranslateMessage(ref msg);
                        DispatchMessage(ref msg);
                    }

                    if (_hwndMessage != IntPtr.Zero)
                    {
                        DestroyWindow(_hwndMessage);
                        _hwndMessage = IntPtr.Zero;
                    }
                });
            });

            _messageThread.SetApartmentState(ApartmentState.MTA);
            _messageThread.IsBackground = true;
            _messageThread.Start();

            readyEvent.Wait(2000);
            _logger.LogInformation("ShellRecoveryService started listening on HWND 0x{Hwnd:X}", _hwndMessage.ToInt64());
        }

        public void StopListening()
        {
            if (!_isRunning) return;
            _isRunning = false;

            if (_messageThread != null && _messageThread.IsAlive)
            {
                if (_hwndMessage != IntPtr.Zero)
                {
                    NativeMethods.PostMessage(_hwndMessage, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
                }
                _messageThread.Join(1000);
            }
        }

        private IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == _msgTaskbarCreated && _msgTaskbarCreated != 0)
            {
                _logger.LogWarning("Explorer restart detected ('TaskbarCreated' received)");
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    // Delay slightly to allow secondary taskbar windows to be created by Explorer
                    Thread.Sleep(1500);
                    ExplorerRestarted?.Invoke(this, EventArgs.Empty);
                });
            }
            else if (msg == NativeConstants.WM_DISPLAYCHANGE)
            {
                _logger.LogInformation("WM_DISPLAYCHANGE received (Display topology or resolution changed)");
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    Thread.Sleep(500);
                    DisplayTopologyChanged?.Invoke(this, EventArgs.Empty);
                });
            }

            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                StopListening();
            }
        }
    }
}

