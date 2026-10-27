using System;
using YCHTTOPYMIW11.Core.Interop;

namespace YCHTTOPYMIW11.Core.Models
{
    public sealed class TaskbarWindowInfo
    {
        public IntPtr Handle { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public TaskbarRole Role { get; set; }
        public IntPtr MonitorHandle { get; set; }
        public string MonitorDeviceName { get; set; } = string.Empty;
        public RECT Bounds { get; set; }
        public bool IsVisible { get; set; }
        public int Style { get; set; }
        public int ExtendedStyle { get; set; }
        public RECT OriginalBounds { get; set; }

        public override string ToString() =>
            $"Taskbar [0x{Handle:X}] {ClassName} Role: {Role}, Visible: {IsVisible}, Bounds: {Bounds}, Monitor: 0x{MonitorHandle:X}";
    }
}

