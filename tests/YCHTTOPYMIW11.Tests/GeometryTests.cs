using Xunit;
using YCHTTOPYMIW11.Core.Interop;

namespace YCHTTOPYMIW11.Tests
{
    public class GeometryTests
    {
        [Fact]
        public void RECT_CalculatesWidthAndHeightCorrectly()
        {
            var rect = new RECT(0, 0, 2560, 1440);
            Assert.Equal(2560, rect.Width);
            Assert.Equal(1440, rect.Height);
            Assert.False(rect.IsEmpty);
        }

        [Fact]
        public void RECT_HandlesNegativeCoordinatesCorrectly()
        {
            var rect = new RECT(-1920, 0, 0, 1080);
            Assert.Equal(1920, rect.Width);
            Assert.Equal(1080, rect.Height);
            Assert.False(rect.IsEmpty);
        }

        [Fact]
        public void RECT_IdentifiesEmptyRect()
        {
            var empty1 = new RECT(100, 100, 100, 100);
            var empty2 = new RECT(200, 200, 100, 100);
            Assert.True(empty1.IsEmpty);
            Assert.True(empty2.IsEmpty);
        }

        [Fact]
        public void RECT_EqualityAndHashCodeWork()
        {
            var r1 = new RECT(0, 1392, 2560, 1440);
            var r2 = new RECT(0, 1392, 2560, 1440);
            var r3 = new RECT(0, 0, 2560, 1440);

            Assert.Equal(r1, r2);
            Assert.True(r1 == r2);
            Assert.NotEqual(r1, r3);
            Assert.True(r1 != r3);
            Assert.Equal(r1.GetHashCode(), r2.GetHashCode());
        }

        [Fact]
        public void POINT_EqualityAndHashCodeWork()
        {
            var p1 = new POINT(100, 200);
            var p2 = new POINT(100, 200);
            var p3 = new POINT(200, 100);

            Assert.Equal(p1, p2);
            Assert.NotEqual(p1, p3);
            Assert.Equal(p1.GetHashCode(), p2.GetHashCode());
        }
    }
}

