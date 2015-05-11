using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RMDEditor.Rmd;

namespace RMDEditor.Node
{
    public class TextureImageNode : ContainerNode
    {
        public Bitmap Image { get { return _ImageDataNode.Image; } }
        public Bitmap PaletteImage { get { return _ImageDataNode.PaletteImage; } }

        public ImageInfoNode ImageInfoNode { get { return _ImageInfoNode; } }
        private ImageInfoNode _ImageInfoNode = null;
        public ImageDataNode ImageDataNode { get { return _ImageDataNode; } }
        private ImageDataNode _ImageDataNode = null;

        public TextureImageNode(ContainerElement elem)
            : base(elem)
        {
            Text = "Texture Image";

            if (!elem.Elements.Select(x => x.Type).SequenceEqual(new uint[] { 0x1, 0x1 }))
                throw new ArgumentException();

            DataNode oldInfoNode = Nodes[0] as DataNode;
            _ImageInfoNode = new ImageInfoNode(oldInfoNode.DataElement);
            DataNode oldImageNode = Nodes[1] as DataNode;
            _ImageDataNode = new ImageDataNode(oldImageNode.DataElement, _ImageInfoNode.DataElement);

            Nodes.Insert(0, _ImageInfoNode);
            Nodes.Remove(oldInfoNode);
            Nodes.Insert(1, _ImageDataNode);
            Nodes.Remove(oldImageNode);

            _SurrogateObject = new SurrogateDataObject(this);
        }

        public new class SurrogateDataObject : ContainerNode.SurrogateDataObject, IImageSurrogate
        {
            protected TextureImageNode _TextureImageNode = null;
            protected ImageInfoNode _ImageInfoNode = null;
            protected ImageDataNode _ImageDataNode = null;
            

            [Category("Image Info")]
            public Size ImageSize { get { return _ImageInfoNode.ImageSize; } }

            [Category("Image Info")]
            public int BitsPerPixel { get { return _ImageInfoNode.BitsPerPixel; } }

            [Browsable(false)]
            public Bitmap Image { get { return _ImageDataNode.Image; } }

            [Browsable(false)]
            public Bitmap PaletteImage { get { return _ImageDataNode.PaletteImage; } }


            public SurrogateDataObject(TextureImageNode owner)
                : base(owner)
            {
                _TextureImageNode = owner;
                _ImageInfoNode = owner.ImageInfoNode;
                _ImageDataNode = owner.ImageDataNode;
            }
        }
    }
}
