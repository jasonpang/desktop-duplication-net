using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopDuplication
{
    public static class InteropExtensions
    {
        /// <summary>
        /// Converts a RawRectangle to a SharpDX.Rectangle.
        /// </summary>
        public static SharpDX.Rectangle ToSharpDXRectangle(this SharpDX.Mathematics.Interop.RawRectangle rawRectangle)
        {
            return new SharpDX.Rectangle(
                rawRectangle.Left,
                rawRectangle.Top,
                rawRectangle.Right - rawRectangle.Left,
                rawRectangle.Bottom - rawRectangle.Top
            );
        }

        /// <summary>
        /// Converts a RawRectangle to a System.Drawing.Rectangle.
        /// </summary>
        public static System.Drawing.Rectangle ToSystemRectangle(this SharpDX.Mathematics.Interop.RawRectangle rawRectangle)
        {
            return new System.Drawing.Rectangle(
                rawRectangle.Left,
                rawRectangle.Top,
                rawRectangle.Right - rawRectangle.Left,
                rawRectangle.Bottom - rawRectangle.Top
            );
        }

        /// <summary>
        /// Converts a RawPoint to a System.Drawing.Rectangle.
        /// </summary>
        public static System.Drawing.Point ToSystemPoint(this SharpDX.Mathematics.Interop.RawPoint rawPoint)
        {
            return new System.Drawing.Point(rawPoint.X, rawPoint.Y);
        }
    }
}
