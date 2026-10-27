using System;
using System.Collections.Generic;
using YCHTTOPYMIW11.Core.Interop;

namespace YCHTTOPYMIW11.Core.Models
{
    public sealed class DesktopViewInfo
    {
        public IntPtr ProgmanHandle { get; set; }
        public IntPtr WorkerWHandle { get; set; }
        public IntPtr DefViewHandle { get; set; }
        public IntPtr ListViewHandle { get; set; }
        public RECT DefViewBounds { get; set; }
        public RECT DefViewClientRect { get; set; }
        public RECT ListViewBounds { get; set; }
        public RECT ListViewClientRect { get; set; }
        public int ListViewStyle { get; set; }
        public int IconCount { get; set; }
        public int GridSpacingX { get; set; }
        public int GridSpacingY { get; set; }
        public List<POINT> SampleIconPositions { get; } = new List<POINT>();

        public override string ToString() =>
            $"Desktop: DefView [0x{DefViewHandle:X}] Bounds: {DefViewBounds}, ListView [0x{ListViewHandle:X}], Icons: {IconCount}, Grid: {GridSpacingX}x{GridSpacingY}";
    }
}

