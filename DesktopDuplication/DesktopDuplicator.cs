using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Rectangle = SharpDX.Rectangle;

namespace DesktopDuplication
{
    /// <summary>
    /// Provides access to frame-by-frame updates of a particular desktop (i.e. one monitor), with image and cursor information.
    /// </summary>
    public class DesktopDuplicator
    {
        private readonly Device _device;
        private readonly Texture2DDescription _textureDescription;
        private OutputDescription _outputDescription;
        private readonly OutputDuplication _deskDupl;

        private Texture2D _desktopImageTexture;
        private OutputDuplicateFrameInformation _frameInfo;
        private readonly int _whichOutputDevice;

        private Bitmap FinalImage { get; set; }

        /// <summary>
        /// Duplicates the output of the specified monitor.
        /// </summary>
        /// <param name="whichMonitor">The output device to duplicate (i.e. monitor). Begins with zero, which seems to correspond to the primary monitor.</param>
        public DesktopDuplicator(int whichMonitor)
            : this(0, whichMonitor)
        {
        }

        /// <summary>
        /// Duplicates the output of the specified monitor on the specified graphics adapter.
        /// </summary>
        /// <param name="whichGraphicsCardAdapter">The adapter which contains the desired outputs.</param>
        /// <param name="whichOutputDevice">The output device to duplicate (i.e. monitor). Begins with zero, which seems to correspond to the primary monitor.</param>
        public DesktopDuplicator(int whichGraphicsCardAdapter, int whichOutputDevice)
        {
            _whichOutputDevice = whichOutputDevice;
            using (var adapter = CreateAdapter(whichGraphicsCardAdapter))
            {
                _device = new Device(adapter);
                using (var output1 = CreateOutput1(whichOutputDevice, adapter))
                {
                    _outputDescription = output1.Description;
                    var desktopBoundsRectangle = _outputDescription.DesktopBounds.ToSharpDXRectangle();
                    _textureDescription = new Texture2DDescription()
                    {
                        CpuAccessFlags = CpuAccessFlags.Read,
                        BindFlags = BindFlags.None,
                        Format = Format.B8G8R8A8_UNorm,
                        Width = desktopBoundsRectangle.Width,
                        Height = desktopBoundsRectangle.Height,
                        OptionFlags = ResourceOptionFlags.None,
                        MipLevels = 1,
                        ArraySize = 1,
                        SampleDescription = {Count = 1, Quality = 0},
                        Usage = ResourceUsage.Staging
                    };

                    _deskDupl = CreateOutputDuplication(output1);
                }
            }
        }

        private OutputDuplication CreateOutputDuplication(Output1 output1)
        {
            try
            {
                return output1.DuplicateOutput(_device);
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode.Code == SharpDX.DXGI.ResultCode.NotCurrentlyAvailable.Result.Code)
                {
                    throw new DesktopDuplicationException(
                        "There is already the maximum number of applications using the Desktop Duplication API running, please close one of the applications and try again.");
                }
                throw;
            }
        }

        private static Output1 CreateOutput1(int whichOutputDevice, Adapter1 adapter)
        {
            try
            {
                return adapter.GetOutput(whichOutputDevice).QueryInterface<Output1>();
            }
            catch (SharpDXException)
            {
                throw new DesktopDuplicationException("Could not find the specified output device.");
            }
        }

        private static Adapter1 CreateAdapter(int whichGraphicsCardAdapter)
        {
            try
            {
                return new Factory1().GetAdapter1(whichGraphicsCardAdapter);
            }
            catch (SharpDXException)
            {
                throw new DesktopDuplicationException("Could not find the specified graphics card adapter.");
            }
        }

        /// <summary>
        /// Retrieves the latest desktop image and associated metadata.
        /// </summary>
        public DesktopFrame GetLatestFrame()
        {
            // Try to get the latest frame; this may timeout
            var retrievalTimedOut = RetrieveFrame();
            if (retrievalTimedOut)
                return null;

            var frame = new DesktopFrame();
            try
            {
                RetrieveFrameMetadata(frame);
                RetrieveCursorMetadata(frame);
                if (frame.MovedRegions.Length != 0 || frame.UpdatedRegions.Length != 0)
                    ProcessFrame(frame);
            }
            catch
            {
                // ignored
            }
            try
            {
                ReleaseFrame();
            }
            catch
            {
                //    throw new DesktopDuplicationException("Couldn't release frame.");  
            }
            return frame;
        }

        private bool RetrieveFrame()
        {
            try
            {
                _frameInfo = new OutputDuplicateFrameInformation();
                SharpDX.DXGI.Resource desktopResource;
                _deskDupl.AcquireNextFrame(500, out _frameInfo, out desktopResource);
                using (desktopResource)
                {
                    if (_desktopImageTexture == null)
                        _desktopImageTexture = new Texture2D(_device, _textureDescription);

                    using (var tempTexture = desktopResource.QueryInterface<Texture2D>())
                        _device.ImmediateContext.CopyResource(tempTexture, _desktopImageTexture);
                }
                return false;
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode.Code == SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                {
                    return true;
                }
                if (ex.ResultCode.Failure)
                {
                    throw new DesktopDuplicationException("Failed to acquire next frame.");
                }
                return false;
            }
        }

        private void ReleaseFrame()
        {
            try
            {
                _deskDupl.ReleaseFrame();
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode.Failure)
                {
                    throw new DesktopDuplicationException("Failed to release frame.");
                }
            }
        }

        private void RetrieveFrameMetadata(DesktopFrame frame)
        {
            if (_frameInfo.TotalMetadataBufferSize > 0)
            {
                // Get moved regions
                int movedRegionsLength;
                var movedRectangles = new OutputDuplicateMoveRectangle[_frameInfo.TotalMetadataBufferSize];
                _deskDupl.GetFrameMoveRects(movedRectangles.Length, movedRectangles, out movedRegionsLength);
                frame.MovedRegions =
                    new MovedRegion[movedRegionsLength / Marshal.SizeOf(typeof(OutputDuplicateMoveRectangle))];
                for (var i = 0; i < frame.MovedRegions.Length; i++)
                {
                    frame.MovedRegions[i] = new MovedRegion()
                    {
                        Source = movedRectangles[i].SourcePoint.ToSystemPoint(),
                        Destination = movedRectangles[i].DestinationRect.ToSystemRectangle()
                    };
                }

                // Get dirty regions
                int dirtyRegionsLength;
                var dirtyRectangles = new RawRectangle[_frameInfo.TotalMetadataBufferSize];
                _deskDupl.GetFrameDirtyRects(dirtyRectangles.Length, dirtyRectangles, out dirtyRegionsLength);
                frame.UpdatedRegions =
                    new System.Drawing.Rectangle[dirtyRegionsLength / Marshal.SizeOf(typeof(Rectangle))];
                for (var i = 0; i < frame.UpdatedRegions.Length; i++)
                {
                    frame.UpdatedRegions[i] = dirtyRectangles[i].ToSystemRectangle();
                }
            }
            else
            {
                frame.MovedRegions = new MovedRegion[0];
                frame.UpdatedRegions = new System.Drawing.Rectangle[0];
            }
        }

        private static readonly byte[] BitPalette = {0, 0, 0, 0, 255, 255, 255, 0};
        private static readonly byte[] NoPalette = new byte[0];

        private void RetrieveCursorMetadata(DesktopFrame frame)
        {
            var pointerInfo = new PointerInfo();

            // A non-zero mouse update timestamp indicates that there is a mouse position update and optionally a shape change
            //if (frameInfo.LastMouseUpdateTime == 0)
            //    return;

            var updatePosition = _frameInfo.PointerPosition.Visible ||
                                 (pointerInfo.WhoUpdatedPositionLast == _whichOutputDevice);

            // If two outputs both say they have a visible, only update if new update has newer timestamp
            if (_frameInfo.PointerPosition.Visible && pointerInfo.Visible &&
                (pointerInfo.WhoUpdatedPositionLast != _whichOutputDevice) &&
                (pointerInfo.LastTimeStamp > _frameInfo.LastMouseUpdateTime))
                updatePosition = false;

            // Update position
            if (updatePosition)
            {
                pointerInfo.Position = _frameInfo.PointerPosition.Position;
                pointerInfo.WhoUpdatedPositionLast = _whichOutputDevice;
                pointerInfo.LastTimeStamp = _frameInfo.LastMouseUpdateTime;
                pointerInfo.Visible = _frameInfo.PointerPosition.Visible;
            }

            // No new shape
            if (_frameInfo.PointerShapeBufferSize != 0)
            {
                try
                {
                    frame.CursorIcon = ExtractIcon(pointerInfo);
                    frame.CursorSize = new Size(pointerInfo.ShapeInfo.Width, pointerInfo.ShapeInfo.Type == 1 ? pointerInfo.ShapeInfo.Height / 2 : pointerInfo.ShapeInfo.Height);
                }
                catch (SharpDXException ex)
                {
                    if (ex.ResultCode.Failure)
                    {
                        throw new DesktopDuplicationException("Failed to get frame pointer shape.");
                    }
                }
            }

            //frame.CursorVisible = pointerInfo.Visible;
            frame.CursorLocation = new System.Drawing.Point(pointerInfo.Position.X, pointerInfo.Position.Y);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public Int32 x;
            public Int32 y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CURSORINFO
        {
            public Int32 cbSize;        // Specifies the size, in bytes, of the structure. 
            public Int32 flags;         // Specifies the cursor state. This parameter can be one of the following values:
            public IntPtr hCursor;          // Handle to the cursor. 
            public POINT ptScreenPos;       // A POINT structure that receives the screen coordinates of the cursor. 
        }

        [DllImport("user32.dll", EntryPoint = "GetCursorInfo")]
        public static extern bool GetCursorInfo(out CURSORINFO pci);

        private unsafe Cursor ExtractIcon(PointerInfo pointerInfo)
        {
            if (_frameInfo.PointerShapeBufferSize > pointerInfo.PtrShapeBuffer.Length)
            {
                pointerInfo.PtrShapeBuffer = new byte[_frameInfo.PointerShapeBufferSize];
            }

            //int bufferSize;
            fixed (byte* ptrShapeBufferPtr = pointerInfo.PtrShapeBuffer)
            {
                _deskDupl.GetFramePointerShape(_frameInfo.PointerShapeBufferSize, (IntPtr)ptrShapeBufferPtr,
                    out int bufferSize, out pointerInfo.ShapeInfo);
            }

 
            var ci = new CURSORINFO();
            ci.cbSize = Marshal.SizeOf(ci);
            if (GetCursorInfo(out ci))
            {
                return new Cursor(ci.hCursor);
            }
            return null;
            /*
            var width = pointerInfo.ShapeInfo.Width;
            var height = pointerInfo.ShapeInfo.Height;


            var inputArray = pointerInfo.PtrShapeBuffer;
            var linePitch = pointerInfo.ShapeInfo.Pitch;

            //var maskPitch = (width + 7) >> 3;
            var imageSize = bufferSize; // pointerInfo.ShapeInfo.Type == 4 ? maskPitch * height * 2 : bufferSize;}}

            var imageArray = new byte[imageSize];
            //if (pointerInfo.ShapeInfo.Type != 4)
            {
                // invert line order or Icon will be upside down
                for (var y = 0; y < height; y++)
                {
                    Array.Copy(inputArray, y * linePitch, imageArray, (height - y - 1) * linePitch,
                        linePitch);
                }
            }
            /*else
            {
                // convert color mask to bitmask
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width;)
                    {
                        byte andMask = 0;
                        byte xorMask = 0;
                        for (var bit = 0; bit < 8 && x < width; x++, bit++)
                        {
                            var offset = (height - y - 1) * linePitch + x * 4;
                            if (inputArray[offset + 0] != 0 || inputArray[offset + 1] != 0 ||
                                inputArray[offset + 2] != 0)
                                xorMask |= (byte) (1 << bit);
                            if (inputArray[offset + 3] != 0)
                                andMask |= (byte) (1 << bit);
                        }
                        imageArray[y * maskPitch + ((x - 1) >> 3)] = xorMask;
                        imageArray[imageSize / 2 + y * maskPitch + ((x - 1) >> 3)] = andMask;
                    }
                }
            }* /


            if (pointerInfo.ShapeInfo.Type == 1)
            {
                var iconBuilder = new IconBuilder
                {
                    BmpHeader =
                    {
                        Width = (uint) width,
                        Height = (uint) height / 2,
                        BitCount = (ushort) 1,
                        ClrUsed = (byte) 2,
                    },
                    Palette =
                    {
                        Data = BitPalette
                    },
                    Image =
                    {
                        Data = imageArray
                    }
                };

                return iconBuilder.Build();
            }
            else if (pointerInfo.ShapeInfo.Type == 2)
            {
                var iconBuilder = new IconBuilder
                {
                    BmpHeader =
                    {
                        Width = (uint) width,
                        Height = (uint) height,
                        BitCount = (ushort) 32,
                    },
                    Image =
                    {
                        Data = imageArray
                    }
                };

                return iconBuilder.Build();
            }
            else //if (pointerInfo.ShapeInfo.Type == 4)
            {
                var iconBuilder = new IconBuilder
                {
                    BmpHeader =
                    {
                        Width = (uint) width,
                        Height = (uint) height,
                        BitCount = (ushort) 24,
                        ClrUsed = (byte) 2,
                    },
                    Image =
                    {
                        Data = imageArray
                    }
                };

                return iconBuilder.Build().;
            }*/
        }

        private void ProcessFrame(DesktopFrame frame)
        {
            // Get the desktop capture texture
            var mapSource = _device.ImmediateContext.MapSubresource(_desktopImageTexture, 0, MapMode.Read, MapFlags.None);

            var desktopBoundsRectangle = _outputDescription.DesktopBounds.ToSystemRectangle();
            FinalImage = new Bitmap(desktopBoundsRectangle.Width, desktopBoundsRectangle.Height,
                PixelFormat.Format32bppRgb);
            var boundsRect = new System.Drawing.Rectangle(0, 0, desktopBoundsRectangle.Width,
                desktopBoundsRectangle.Height);
            // Copy pixels from screen capture Texture to GDI bitmap
            var mapDest = FinalImage.LockBits(boundsRect, ImageLockMode.WriteOnly, FinalImage.PixelFormat);
            var sourcePtr = mapSource.DataPointer;
            var destPtr = mapDest.Scan0;
            for (var y = 0; y < desktopBoundsRectangle.Height; y++)
            {
                // Copy a single line 
                Utilities.CopyMemory(destPtr, sourcePtr, desktopBoundsRectangle.Width * 4);

                // Advance pointers
                sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                destPtr = IntPtr.Add(destPtr, mapDest.Stride);
            }

            // Release source and dest locks
            FinalImage.UnlockBits(mapDest);
            _device.ImmediateContext.UnmapSubresource(_desktopImageTexture, 0);
            frame.DesktopImage = FinalImage;
        }
    }
}