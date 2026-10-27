using System;

namespace YCHTTOPYMIW11.Core.Interop
{
    public static class NativeConstants
    {
        // SystemParametersInfo Actions
        public const uint SPI_GETWORKAREA = 0x0030;
        public const uint SPI_SETWORKAREA = 0x002F;
        public const uint SPIF_UPDATEINIFILE = 0x0001;
        public const uint SPIF_SENDCHANGE = 0x0002;

        // Window Messages
        public const uint WM_SETTINGCHANGE = 0x001A;
        public const uint WM_WININICHANGE = 0x001A;
        public const uint WM_DISPLAYCHANGE = 0x007E;
        public const uint WM_DPICHANGED = 0x02E0;
        public const uint WM_COMMAND = 0x0111;
        public const uint WM_USER = 0x0400;

        // AppBar Messages
        public const uint ABM_NEW = 0x00000000;
        public const uint ABM_REMOVE = 0x00000001;
        public const uint ABM_QUERYPOS = 0x00000002;
        public const uint ABM_SETPOS = 0x00000003;
        public const uint ABM_GETSTATE = 0x00000004;
        public const uint ABM_GETTASKBARPOS = 0x00000005;
        public const uint ABM_ACTIVATE = 0x00000006;
        public const uint ABM_GETAUTOHIDEBAR = 0x00000007;
        public const uint ABM_SETAUTOHIDEBAR = 0x00000008;
        public const uint ABM_WINDOWPOSCHANGED = 0x00000009;
        public const uint ABM_SETSTATE = 0x0000000a;

        // AppBar Edges
        public const uint ABE_LEFT = 0;
        public const uint ABE_TOP = 1;
        public const uint ABE_RIGHT = 2;
        public const uint ABE_BOTTOM = 3;

        // AppBar States
        public const uint ABS_MANUAL = 0x00000000;
        public const uint ABS_AUTOHIDE = 0x00000001;
        public const uint ABS_ALWAYSONTOP = 0x00000002;

        // ShowWindow Commands
        public const int SW_HIDE = 0;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOW = 5;

        // SetWindowPos Flags
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_NOREDRAW = 0x0008;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_FRAMECHANGED = 0x0020;
        public const uint SWP_SHOWWINDOW = 0x0040;
        public const uint SWP_HIDEWINDOW = 0x0080;
        public const uint SWP_NOCOPYBITS = 0x0100;
        public const uint SWP_NOOWNERZORDER = 0x0200;
        public const uint SWP_NOSENDCHANGING = 0x0400;

        // MonitorFromWindow Flags
        public const uint MONITOR_DEFAULTTONULL = 0;
        public const uint MONITOR_DEFAULTTOPRIMARY = 1;
        public const uint MONITOR_DEFAULTTONEAREST = 2;

        // Monitor Info Flags
        public const uint MONITORINFOF_PRIMARY = 0x00000001;

        // GetWindowLong Indexes
        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;

        // Window Styles
        public const uint WS_POPUP = 0x80000000;
        public const uint WS_VISIBLE = 0x10000000;
        public const uint WS_CLIPSIBLINGS = 0x04000000;
        public const uint WS_CLIPCHILDREN = 0x02000000;

        // Extended Window Styles
        public const uint WS_EX_TOPMOST = 0x00000008;
        public const uint WS_EX_TOOLWINDOW = 0x00000080;
        public const uint WS_EX_TRANSPARENT = 0x00000020;
        public const uint WS_EX_LAYERED = 0x00080000;

        // DWM Window Attributes
        public const uint DWMWA_CLOAK = 13;
        public const uint DWMWA_CLOAKED = 14;

        // ListView Messages for Desktop Icon View
        public const uint LVM_FIRST = 0x1000;
        public const uint LVM_GETITEMCOUNT = LVM_FIRST + 4;
        public const uint LVM_GETITEMPOSITION = LVM_FIRST + 16;
        public const uint LVM_SETITEMPOSITION = LVM_FIRST + 15;
        public const uint LVM_GETITEMSPACING = LVM_FIRST + 51;

        // WinEvent Hooks
        public const uint EVENT_OBJECT_SHOW = 0x8002;
        public const uint EVENT_OBJECT_HIDE = 0x8003;
        public const uint WINEVENT_OUTOFCONTEXT = 0x0000;
        public const uint WINEVENT_SKIPOWNPROCESS = 0x0002;

        // Desktop Security
        public const uint DESKTOP_READOBJECTS = 0x0001;
        public const uint DESKTOP_CREATEWINDOW = 0x0002;
        public const uint DESKTOP_CREATEMENU = 0x0004;
        public const uint DESKTOP_HOOKCONTROL = 0x0008;
        public const uint DESKTOP_JOURNALRECORD = 0x0010;
        public const uint DESKTOP_JOURNALPLAYBACK = 0x0020;
        public const uint DESKTOP_ENUMERATE = 0x0040;
        public const uint DESKTOP_WRITEOBJECTS = 0x0080;
        public const uint DESKTOP_SWITCHDESKTOP = 0x0100;
        public const uint GENERIC_ALL_DESKTOP = 0x01FF;

        // Special HWND values
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        public static readonly IntPtr HWND_TOP = new IntPtr(0);
        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        public static readonly IntPtr HWND_BROADCAST = new IntPtr(0xFFFF);
    }
}

