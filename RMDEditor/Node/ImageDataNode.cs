using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using RMDEditor.Rmd;

using ImageInfoWrapper = RMDEditor.Node.ImageInfoNode.ImageInfoWrapper;

namespace RMDEditor.Node
{
    public unsafe class ImageDataNode : DataNode
    {
        public Bitmap Image { get { return _Image; } }
        private Bitmap _Image = null;

        public Color[] Palette { get { return _Palette; } }
        private Color[] _Palette = null;

        public Bitmap PaletteImage { get { return _PaletteImage; } }
        private Bitmap _PaletteImage = null;

        public ImageDataNode(DataElement elem, DataElement info)
            : base(elem)
        {
            ImageInfoWrapper* imageInfo = (ImageInfoWrapper*)info.Data;

            if (imageInfo->bitsPerPixel == 0x20)
                Parse32BppRGB(info);
            else if (imageInfo->bitsPerPixel == 0x8)
                Parse8BppIndexed(info);
            else if (imageInfo->bitsPerPixel == 0x4)
                Parse4BppIndexed(info);
            else
                throw new Exception(string.Format("Unable to parse {0}-bit per pixel image.", imageInfo->bitsPerPixel));

            _SurrogateObject = new SurrogateDataObject(this);
            Text = "Image Data";
        }

        private void Parse32BppRGB(DataElement info)
        {
            byte* ptr = _DataElement.Data;
            ImageInfoWrapper* imageInfo = (ImageInfoWrapper*)info.Data;
            ImageDataHeaderWrapper* imageHeader = (ImageDataHeaderWrapper*)ptr;
            byte* imageData = (byte*)imageHeader + ImageDataHeaderWrapper.SIZE;

            _Image = new Bitmap(imageHeader->width, imageHeader->height, PixelFormat.Format32bppRgb);


            for (int y = 0; y < _Image.Height; y++)
                for (int x = 0; x < _Image.Width; x++)
                {
                    int index = (y * _Image.Width + x) * sizeof(uint);
                    int r = imageData[index + 0x0];
                    int g = imageData[index + 0x1];
                    int b = imageData[index + 0x2];
                    int a = imageData[index + 0x3];
                    _Image.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                }
        }

        private void Parse8BppIndexed(DataElement info)
        {
            byte* ptr = _DataElement.Data;
            ImageInfoWrapper* imageInfo = (ImageInfoWrapper*)info.Data;
            ImageDataHeaderWrapper* imageHeader = (ImageDataHeaderWrapper*)ptr;
            ImageDataHeaderWrapper* paletteHeader = (ImageDataHeaderWrapper*)(ptr + imageInfo->imageDataSize);
            byte* imageData = (byte*)imageHeader + ImageDataHeaderWrapper.SIZE;
            byte* paletteData = (byte*)paletteHeader + ImageDataHeaderWrapper.SIZE;

            _Image = new Bitmap(imageInfo->width, imageInfo->height, PixelFormat.Format32bppRgb);
            _PaletteImage = new Bitmap(0x10, 0x10, PixelFormat.Format32bppRgb);
            _Palette = new Color[0x100];

            for (int i = 0; i < _Palette.Length; i++)
            {
                int index = i & ~0x18 | ((i & 0x10) >> 0x1) | ((i & 0x8) << 0x1);

                _Palette[i] = Color.FromArgb(paletteData[(index * 4) + 0], paletteData[(index * 4) + 1], paletteData[(index * 4) + 2]); ;
                _PaletteImage.SetPixel(i % 0x10, i / 0x10, _Palette[i]);   
            }
            
            for (int by = 0; by < _Image.Height / 0x2; by++)
            for (int bx = 0; bx < _Image.Width / 0x10; bx++)
            {
                int blockY = (by / 0x2) * 0x4 + (by % 0x2);
                int blockX = bx * 0x10;
                int blockI = bx * 0x20 + by * _Image.Width * 0x2;

                for (int j = 0; j < 0x8; j++)
                {
                    int x = (blockX + j) ^ ((by & 0x2) << 1);
                    int x2 = (blockX + 0x8 + j) ^ ((by & 0x2) << 1);
                    int y = blockY + 0x0;
                    int y2 = blockY + 0x2;
                    int i1 = blockI + (j * 0x4) % 0x20;
                    int i2 = blockI + (j * 0x4 + 0x11) % 0x20;
                    int i3 = blockI + (j * 0x4 + 0x2) % 0x20;
                    int i4 = blockI + (j * 0x4 + 0x11 + 0x2) % 0x20;

                    _Image.SetPixel(x, y, _Palette[imageData[i1]]);
                    _Image.SetPixel(x, y2, _Palette[imageData[i2]]);
                    _Image.SetPixel(x2, y, _Palette[imageData[i3]]);
                    _Image.SetPixel(x2, y2, _Palette[imageData[i4]]);
                }
            }
            
            /*
            for (int y = 0; y < _Image.Height ; y++)
                for (int x = 0; x < _Image.Width; x++)
                {
                    int index = (y * _Image.Width + x);
                    int i = imageData[index];
                    //int r = imageData[index + 0x0];
                    //int g = imageData[index + 0x1];
                    //int b = imageData[index + 0x2];
                    //int a = imageData[index + 0x3];
                    _Image.SetPixel(x, y, _Palette[i]);
                }
             */
        }

        private void Parse4BppIndexed(DataElement info)
        {
            byte* ptr = _DataElement.Data;
            ImageInfoWrapper* imageInfo = (ImageInfoWrapper*)info.Data;
            ImageDataHeaderWrapper* imageHeader = (ImageDataHeaderWrapper*)ptr;
            ImageDataHeaderWrapper* paletteHeader = (ImageDataHeaderWrapper*)(ptr + imageInfo->imageDataSize);
            byte* imageData = (byte*)imageHeader + ImageDataHeaderWrapper.SIZE;
            byte* paletteData = (byte*)paletteHeader + ImageDataHeaderWrapper.SIZE;

            _Image = new Bitmap(imageInfo->width, imageInfo->height, PixelFormat.Format32bppRgb);
            _PaletteImage = new Bitmap(0x4, 0x4, PixelFormat.Format32bppRgb);
            _Palette = new Color[0x10];

            for (int i = 0; i < _Palette.Length; i++)
            {
                _Palette[i] = Color.FromArgb(paletteData[(i * 4) + 0], paletteData[(i * 4) + 1], paletteData[(i * 4) + 2]); ;
                _PaletteImage.SetPixel(i % 0x4, i / 0x4, _Palette[i]);
            }

            for (int by = 0; by < _Image.Height / 0x2; by++)
                for (int bx = 0; bx < _Image.Width / 0x10; bx++)
                {
                    int blockY = (by / 0x2) * 0x4 + (by % 0x2);
                    int blockX = bx * 0x10;
                    int blockI = bx * 0x20 + by * Math.Max(_Image.Width, 0x20) * 0x2;

                    for (int j = 0; j < 0x8; j++)
                    {
                        int x = (blockX + (0x4 + j) % 0x8) ^ ((by & 0x2) << 1);
                        int x2 = (blockX + 0x8 + (0x4 + j) % 0x8) ^ ((by & 0x2) << 1);
                        int y = blockY + 0x0;
                        int y2 = blockY + 0x2;
                        int i1 = blockI + (j * 0x4) % 0x20;
                        int i2 = blockI + (j * 0x4 + 0x11) % 0x20;
                        int i3 = blockI + (j * 0x4 + 0x2) % 0x20;
                        int i4 = blockI + (j * 0x4 + 0x11 + 0x2) % 0x20;

                        _Image.SetPixel(x, y2, _Palette[ReadNibble(imageData, i1)]);
                        _Image.SetPixel(x, y, _Palette[ReadNibble(imageData, i2)]);
                        _Image.SetPixel(x2, y2, _Palette[ReadNibble(imageData, i3)]);
                        _Image.SetPixel(x2, y, _Palette[ReadNibble(imageData, i4)]);
                    }
                }
        }

        private int ReadNibble(byte* ptr, int index)
        {
            int byteIndex = index / 0x2;
            bool bitShift = (index % 0x2) == 0;

            return (ptr[byteIndex] >> (bitShift ? 0x4 : 0x0)) & 0xF;
        }

        public new class SurrogateDataObject : DataNode.SurrogateDataObject, IImageSurrogate
        {
            private ImageDataNode _ImageDataNode = null;

            [Browsable(false)]
            public Bitmap Image { get { return _ImageDataNode.Image; } }

            [Browsable(false)]
            public Bitmap PaletteImage { get { return _ImageDataNode.PaletteImage; } }

            public SurrogateDataObject(ImageDataNode owner)
                : base(owner)
            {
                _ImageDataNode = owner;
            }
        }

        #region Wrapper Classes

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct ImageDataHeaderWrapper
        {
            public const int SIZE = 0x50;

            public uint unknown1;
            public uint unknown2;
            public uint unknown3;
            public uint unknown4;
            public uint unknown5;
            public uint unknown6;
            public uint unknown7;
            public uint unknown8;
            public int width;
            public int height;
            public uint unknown9;
            public uint unknown10;
            public uint unknown11;
            public uint unknown12;
            public uint unknown13;
            public uint unknown14;
            public uint unknown15;
            public uint unknown16;
            public uint unknown17;
            public uint unknown18;

        }

        #endregion
    }
}
