using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RMDEditor.Rmd;

namespace RMDEditor.Node
{
    public unsafe class TextureSetNode : ContainerNode
    {
        public int TextureCount { get { return GetTextureCount(); } }

        public TextureSetNode(ContainerElement elem)
            : base(elem)
        {
            Text = "Textures";

            if (RmdNodes.Length > 0 && RmdNodes.First().ElementType == 0x01)
                RmdNodes.First().Text = "Count Node";

            if (RmdNodes.Length > 1 && RmdNodes.Last().ElementType == 0x03)
                RmdNodes.Last().Text = "Ex Data";

            _SurrogateObject = new SurrogateDataObject(this);
        }

        public int GetTextureCount()
        {
            DataElement data = _ContainerElement.Elements[0] as DataElement;

            return *(ushort*)data.Data;
        } 

        public new class SurrogateDataObject : ContainerNode.SurrogateDataObject
        {
            protected TextureSetNode _TextureNode = null;

            [Category("Textures")]
            public int TextureCount { get { return _TextureNode.TextureCount; } }

            public SurrogateDataObject(TextureSetNode owner)
                : base(owner)
            {
                _TextureNode = owner;
            }
        }
    }
}
