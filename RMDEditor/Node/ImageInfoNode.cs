using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using RMDEditor.Rmd;

namespace RMDEditor.Node
{
    public unsafe class ImageInfoNode : DataNode
    {
        private ImageInfoWrapper* _ImageInfo = null;

        public int ImageWidth { get { return _ImageInfo->width; } }
        public int ImageHeight { get { return _ImageInfo->height; } }
        public Size ImageSize { get { return new Size(ImageWidth, ImageHeight); } }

        public int BitsPerPixel { get { return _ImageInfo->bitsPerPixel; } }

        public ImageInfoNode(DataElement elem)
            : base(elem)
        {
            _SurrogateObject = new SurrogateDataObject(this);
            Text = "Image Info";

            _ImageInfo = (ImageInfoWrapper*)elem.Data;
        }

        public new class SurrogateDataObject : DataNode.SurrogateDataObject
        {
            private ImageInfoNode _ImageInfoNode = null;

            [Category("Image Info")]
            public Size ImageSize { get { return _ImageInfoNode.ImageSize; } }

            [Category("Image Info")]
            public int BitsPerPixel { get { return _ImageInfoNode.BitsPerPixel; } }

            public SurrogateDataObject(ImageInfoNode owner)
                : base(owner)
            {
                _ImageInfoNode = owner;
            }
        }

        #region Wrapper Classes

        [StructLayout(LayoutKind.Sequential, Pack=0)]
        public struct ImageInfoWrapper
        {
            public int width;
            public int height;
            public int bitsPerPixel;
            public uint unk2;

            public uint unk3;
            public uint unk4;
            public uint unk5;
            public uint unk6;

            public uint unk7;
            public uint unk8;
            public uint unk9;
            public uint unk10;

            public int imageDataSize;
            public int paletteDataSize;

            public uint unk11;
            public uint unk12;
        }

        #endregion
    }
}
