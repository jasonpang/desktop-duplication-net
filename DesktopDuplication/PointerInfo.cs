using SharpDX.DXGI;

namespace DesktopDuplication
{
    internal class PointerInfo
    {
        public byte[] PtrShapeBuffer = new byte[1024];
        public OutputDuplicatePointerShapeInformation ShapeInfo;
        public SharpDX.Point Position;
        public bool Visible;
        public int WhoUpdatedPositionLast;
        public long LastTimeStamp;
    }
}
