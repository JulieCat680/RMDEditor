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
    public unsafe class TextureRefNode : ContainerNode
    {
        public string TextureName { get { return GetTextureName(); } set { SetTextureName(value); } }

        public TextureRefNode(ContainerElement elem)
            : base(elem)
        {
            Text = GetTextureName();

            if (!elem.Elements.Select(x => x.Type).SequenceEqual(new uint[] { 0x1, 0x2, 0x2, 0x3 }))
                throw new ArgumentException();

            Nodes[0].Text = "Usage Data";
            Nodes[1].Text = "Name Data";
            Nodes[2].Text = "Unknown";
            Nodes[3].Text = "Ex Data";

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

        public new class SurrogateDataObject : ContainerNode.SurrogateDataObject
        {
            protected TextureRefNode _TextureRefNode = null;

            [Category("Texture Reference")]
            public string Texture { get { return _TextureRefNode.TextureName; } set { SetTextureName(value); } }

            public SurrogateDataObject(TextureRefNode owner)
                : base(owner)
            {
                _TextureRefNode = owner;
            }

            private void SetTextureName(string value)
            {
                if (_TextureRefNode.Parent is MaterialNode)
                    (_TextureRefNode.Parent as MaterialNode).SetTextureName(value);
                else
                    _TextureRefNode.SetTextureName(value);
            }
        }
    }
}
