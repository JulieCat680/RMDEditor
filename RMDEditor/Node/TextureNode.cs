using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using RMDEditor.Rmd;

namespace RMDEditor.Node
{
    public unsafe class TextureNode : ContainerNode
    {
        public TextureImageNode TextureImageNode { get { return _TextureImageNode; } }
        private TextureImageNode _TextureImageNode = null;

        public string TextureName { get { return GetTextureName(); } set { SetTextureName(value); } }

        public TextureNode(ContainerElement elem)
            : base(elem)
        {
            Text = GetTextureName();

            if (!elem.Elements.Select(x => x.Type).SequenceEqual(new uint[] { 0x1, 0x2, 0x2, 0x1, 0x3 }))
                throw new ArgumentException();

            RmdNode oldNode = RmdNodes[3];
            _TextureImageNode = new TextureImageNode(oldNode.Element as ContainerElement);
            Nodes.Insert(3, _TextureImageNode);
            Nodes.Remove(oldNode);

            Nodes[0].Text = "Usage Data";
            Nodes[1].Text = "Name Data";
            Nodes[2].Text = "Unknown";
            Nodes[4].Text = "Ex Data";

            _SurrogateObject = new SurrogateDataObject(this);
        }

        public string GetTextureName()
        {
            ContainerElement container = _ContainerElement;
            DataElement nameData = container.Elements[1] as DataElement;

            return new string((sbyte*)nameData.Data);
        }

        public void SetTextureName(string value)
        {
            if (value == null)
                value = "";

            ContainerElement container = _ContainerElement;
            DataElement nameData = container.Elements[1] as DataElement;
            byte[] data = Encoding.ASCII.GetBytes(value + '\0');
            int size = (data.Length + 0x3) & ~0x3;

            nameData.Resize(size);
            for (int i = 0; i < size; i++)
                if (i < data.Length)
                    nameData.Data[i] = data[i];
                else
                    nameData.Data[i] = 0;

            Text = value;
        }

        public new class SurrogateDataObject : ContainerNode.SurrogateDataObject, IImageSurrogate
        {
            protected TextureNode _TextureNode = null;
            protected TextureImageNode _TextureImageNode = null;
            protected ImageInfoNode _ImageInfoNode = null;
            protected ImageDataNode _ImageDataNode = null;

            [Category("Texture")]
            public string Name { get { return _TextureNode.GetTextureName(); } set { _TextureNode.TextureName = value; } }

            [Category("Image Info")]
            public Size ImageSize { get { return _ImageInfoNode.ImageSize; } }

            [Category("Image Info")]
            public int BitsPerPixel { get { return _ImageInfoNode.BitsPerPixel; } }

            [Browsable(false)]
            public Bitmap Image { get { return _TextureImageNode.ImageDataNode.Image; } }

            [Browsable(false)]
            public Bitmap PaletteImage { get { return _TextureImageNode.ImageDataNode.PaletteImage; } }

            public SurrogateDataObject(TextureNode owner)
                : base(owner)
            {
                _TextureNode = owner;
                _TextureImageNode = owner.TextureImageNode;
                _ImageInfoNode = _TextureImageNode.ImageInfoNode;
                _ImageDataNode = _TextureImageNode.ImageDataNode;
            }
        }
    }
}
