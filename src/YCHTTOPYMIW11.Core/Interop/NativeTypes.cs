using System;
using System.Runtime.InteropServices;

namespace YCHTTOPYMIW11.Core.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT : IEquatable<RECT>
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int Width => Right - Left;
        public int Height => Bottom - Top;

        public bool IsEmpty => Width <= 0 || Height <= 0;

        public bool Equals(RECT other) =>
            Left == other.Left && Top == other.Top && Right == other.Right && Bottom == other.Bottom;

        public override bool Equals(object? obj) => obj is RECT other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Left, Top, Right, Bottom);

        public static bool operator ==(RECT left, RECT right) => left.Equals(right);
        public static bool operator !=(RECT left, RECT right) => !left.Equals(right);

        public override string ToString() => $"({Left}, {Top}, {Right}, {Bottom}) [{Width}x{Height}]";
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT : IEquatable<POINT>
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(POINT other) => X == other.X && Y == other.Y;
        public override bool Equals(object? obj) => obj is POINT other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X}, {Y})";
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct MONITORINFOEX
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szDevice;

        public static MONITORINFOEX Create()
        {
            var mi = new MONITORINFOEX();
            mi.cbSize = Marshal.SizeOf<MONITORINFOEX>();
            mi.szDevice = string.Empty;
            return mi;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct APPBARDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public uint uEdge;
        public RECT rc;
        public IntPtr lParam;

        public static APPBARDATA Create(IntPtr hWnd)
        {
            var data = new APPBARDATA();
            data.cbSize = Marshal.SizeOf<APPBARDATA>();
            data.hWnd = hWnd;
            return data;
        }
    }
}

