using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RMDEditor.Rmd;

namespace RMDEditor.Node
{
    public class MaterialNode : ContainerNode
    {
        public TextureRefNode TextureRefNode { get { return _TextureRefNode; } }
        private TextureRefNode _TextureRefNode = null;

        public string TextureName { get { return GetTextureName(); } set { SetTextureName(value); } }

        public MaterialNode(ContainerElement elem)
            : base(elem)
        {

            if (RmdNodes.Length > 0 && RmdNodes.First().ElementType == 0x1)
                RmdNodes.First().Text = "Material Data";

            _TextureRefNode = Nodes[1] as TextureRefNode;
            if (_TextureRefNode != null)
                _TextureRefNode.Text = "Texture Reference";

            if (RmdNodes.Length > 1 && RmdNodes.Last().ElementType == 0x3)
                RmdNodes.Last().Text = "Ex Data";
            

            Text = string.Format("Material [{0}]", _TextureRefNode.TextureName);
            _SurrogateObject = new SurrogateDataObject(this);
        }

        public string GetTextureName()
        {
            return _TextureRefNode.TextureName;
        }

        public void SetTextureName(string value)
        {
            _TextureRefNode.TextureName = value;
            Text = string.Format("Material [{0}]", value);
        }

        public new class SurrogateDataObject : ContainerNode.SurrogateDataObject
        {
            protected MaterialNode _MaterialNode = null;
            protected TextureRefNode _TextureRefNode = null;

            [Category("Texture Reference")]
            public string Texture { get { return _MaterialNode.TextureName; } set { _MaterialNode.TextureName = value; } }

            public SurrogateDataObject(MaterialNode owner)
                : base(owner)
            {
                _MaterialNode = owner;
                _TextureRefNode = owner._TextureRefNode;
            }
        }
    }
}
