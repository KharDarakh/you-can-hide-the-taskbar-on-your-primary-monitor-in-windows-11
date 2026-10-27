using System;
using YCHTTOPYMIW11.Core.Interop;

namespace YCHTTOPYMIW11.Core.Models
{
    public sealed class DisplayInfo : IEquatable<DisplayInfo>
    {
        public IntPtr MonitorHandle { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string FriendlyName { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public RECT MonitorBounds { get; set; }
        public RECT WorkArea { get; set; }
        public uint DpiX { get; set; } = 96;
        public uint DpiY { get; set; } = 96;
        public double ScaleFactor => Math.Round(DpiX / 96.0, 2);

        public int Width => MonitorBounds.Width;
        public int Height => MonitorBounds.Height;

        public bool Equals(DisplayInfo? other)
        {
            if (other is null) return false;
            return DeviceName.Equals(other.DeviceName, StringComparison.OrdinalIgnoreCase) &&
                   IsPrimary == other.IsPrimary &&
                   MonitorBounds == other.MonitorBounds;
        }

        public override bool Equals(object? obj) => Equals(obj as DisplayInfo);

        public override int GetHashCode() =>
            HashCode.Combine(DeviceName.ToUpperInvariant(), IsPrimary, MonitorBounds);

        public override string ToString() =>
            $"{DeviceName} (Primary: {IsPrimary}) Bounds: {MonitorBounds}, WorkArea: {WorkArea}, DPI: {DpiX} ({ScaleFactor:P0})";
    }
}

