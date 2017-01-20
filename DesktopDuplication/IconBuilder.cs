using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace DesktopDuplication
{
    internal class ICONDIR
    {
        private readonly byte[] _array = new byte[Size];

        public ushort Type
        {
            get { return (ushort) Marshal.ReadInt16(_array, 2); }
            set { Marshal.WriteInt16(_array, 2, (char) value); }
        }

        public ushort Count
        {
            get { return (ushort) Marshal.ReadInt16(_array, 4); }
            set { Marshal.WriteInt16(_array, 4, (char) value); }
        }

        public static int Size => 6;

        public void Write(Stream stream)
        {
            stream.Write(_array, 0, Size);
        }
    }

    internal class ICONDIRENTRY
    {
        private readonly byte[] _array = new byte[Size];

        public byte Width
        {
            get { return Marshal.ReadByte(_array, 0); }
            set { Marshal.WriteByte(_array, 0, value); }
        }

        public byte Height
        {
            get { return Marshal.ReadByte(_array, 1); }
            set { Marshal.WriteByte(_array, 1, value); }
        }

        public byte ColorCount
        {
            get { return Marshal.ReadByte(_array, 2); }
            set { Marshal.WriteByte(_array, 2, value); }
        }

        public ushort Planes
        {
            get { return (ushort) Marshal.ReadInt16(_array, 4); }
            set { Marshal.WriteInt16(_array, 4, (short) value); }
        }

        public ushort BitCount
        {
            get { return (ushort) Marshal.ReadInt16(_array, 6); }
            set { Marshal.WriteInt16(_array, 6, (short) value); }
        }

        public uint BytesInRes
        {
            get { return (uint) Marshal.ReadInt32(_array, 8); }
            set { Marshal.WriteInt32(_array, 8, (int) value); }
        }

        public uint ImageOffset
        {
            get { return (uint) Marshal.ReadInt32(_array, 12); }
            set { Marshal.WriteInt32(_array, 12, (int) value); }
        }

        public static int Size => 16;

        public void Write(Stream stream)
        {
            stream.Write(_array, 0, Size);
        }
    }

    public enum IconImageFormat : int
    {
        BMP = 0
    }

    public class BITMAPINFOHEADER
    {
        private readonly byte[] _array = prefilled();

        private static byte[] prefilled()
        {
            var array = new byte[Size];
            Marshal.WriteInt32(array, 0, Size);
            return array;
        }

        public uint Width
        {
            get { return (uint) Marshal.ReadInt32(_array, 4); }
            set { Marshal.WriteInt32(_array, 4, (int) value); }
        }

        public uint Height
        {
            get { return (uint) Marshal.ReadInt32(_array, 8) / 2; }
            set { Marshal.WriteInt32(_array, 8, (int) value * 2); }
        }

        public ushort Planes
        {
            get { return (ushort) Marshal.ReadInt16(_array, 12); }
            set { Marshal.WriteInt16(_array, 12, (short) value); }
        }

        public ushort BitCount
        {
            get { return (ushort) Marshal.ReadInt16(_array, 14); }
            set { Marshal.WriteInt16(_array, 14, (short) value); }
        }

        public IconImageFormat Compression
        {
            get { return (IconImageFormat) Marshal.ReadInt32(_array, 16); }
            set { Marshal.WriteInt32(_array, 16, (int) value); }
        }

        public uint ImageSize
        {
            get { return (uint) Marshal.ReadInt32(_array, 20); }
            set { Marshal.WriteInt32(_array, 20, (int) value); }
        }

        public int XPelsPerMeter
        {
            get { return Marshal.ReadInt32(_array, 24); }
            set { Marshal.WriteInt32(_array, 24, value); }
        }

        public int YPelsPerMeter
        {
            get { return Marshal.ReadInt32(_array, 28); }
            set { Marshal.WriteInt32(_array, 28, value); }
        }

        public uint ClrUsed
        {
            get { return (uint) Marshal.ReadInt32(_array, 32); }
            set { Marshal.WriteInt32(_array, 32, (int) value); }
        }

        public uint ClrImportant
        {
            get { return (uint) Marshal.ReadInt32(_array, 36); }
            set { Marshal.WriteInt32(_array, 36, (int) value); }
        }

        public static int Size => 40;

        public void Write(Stream stream)
        {
            stream.Write(_array, 0, Size);
        }
    }

    public class PaletteData
    {
        public byte[] Data { get; set; } = new byte[0];

        public int Size => Data.Length;

        public int Count => Data.Length / 4;

        public void Write(Stream stream)
        {
            stream.Write(Data, 0, Size);
        }
    }

    public class ImageData
    {
        public byte[] Data { get; set; } = new byte[0];

        public int Size => Data.Length;

        public void Write(Stream stream)
        {
            stream.Write(Data, 0, Size);
        }
    }

    public class IconBuilder
    {
        public BITMAPINFOHEADER BmpHeader { get; } = new BITMAPINFOHEADER {Planes = 1, Compression = IconImageFormat.BMP};
        public PaletteData Palette { get; } = new PaletteData();
        public ImageData Image { get; } = new ImageData();

        public Icon Build()
        {
            using (var ms = new MemoryStream())
            {
                var iconDir = new ICONDIR
                {
                    Type = 1,
                    Count = 1
                };
                iconDir.Write(ms);

                var iconEntry = new ICONDIRENTRY
                {
                    ColorCount = (byte) BmpHeader.ClrUsed,
                    Height = (byte) BmpHeader.Height,
                    Width = (byte) BmpHeader.Width,
                    BytesInRes = (uint) (BITMAPINFOHEADER.Size + Palette.Size + Image.Size),
                    ImageOffset = (uint) (ICONDIR.Size + ICONDIRENTRY.Size),
                    BitCount = BmpHeader.BitCount,
                    Planes = BmpHeader.Planes
                };
                iconEntry.Write(ms);

                ms.Seek(iconEntry.ImageOffset, SeekOrigin.Begin);
                BmpHeader.ImageSize = (uint) (Palette.Size + Image.Size);
                BmpHeader.Write(ms);
                Palette.Write(ms);
                Image.Write(ms);

                ms.Position = 0;
                return new Icon(ms, iconEntry.Width, iconEntry.Height);
            }
        }
    }
}