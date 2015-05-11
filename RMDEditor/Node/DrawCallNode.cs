using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RMDEditor.Rmd;

namespace RMDEditor.Node
{
    public unsafe class DrawCallNode : ContainerNode
    {
        public int MeshIndex { get { return GetMeshIndex(); } set { SetMeshIndex(value); } }

        public DrawCallNode(ContainerElement elem)
            : base(elem)
        {
            Text = string.Format("Draw Call [{0}]", GetMeshIndex());

            if (RmdNodes.Length > 0 && RmdNodes.First().ElementType == 0x01)
                RmdNodes.First().Text = "Draw Data";

            if (RmdNodes.Length > 1 && RmdNodes.Last().ElementType == 0x03)
                RmdNodes.Last().Text = "Ex Data";

            _SurrogateObject = new SurrogateDataObject(this);
        }

        public int GetMeshIndex()
        {
            DataElement data = _ContainerElement.Elements[0] as DataElement;

            return *(int*)(data.Data + 0x4);
        }

        public void SetMeshIndex(int value)
        {
            DataElement data = _ContainerElement.Elements[0] as DataElement;

            *(int*)(data.Data + 0x4) = value;
            Text = string.Format("Draw Call [{0}]", value);
        }

        public new class SurrogateDataObject : ContainerNode.SurrogateDataObject
        {
            protected DrawCallNode _DrawCallNode = null;

            [Category("Draw Data")]
            public int Mesh { get { return _DrawCallNode.MeshIndex; } set { _DrawCallNode.MeshIndex = value; } }

            public SurrogateDataObject(DrawCallNode owner)
                : base(owner)
            {
                _DrawCallNode = owner;
            }
        }
    }
}
