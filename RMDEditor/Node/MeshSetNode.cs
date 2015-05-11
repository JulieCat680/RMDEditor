using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RMDEditor.Rmd;

namespace RMDEditor.Node
{
    public unsafe class MeshSetNode : ContainerNode
    {
        public int MeshCount { get { return GetMeshCount(); } }

        public MeshSetNode(ContainerElement elem)
            : base(elem)
        {
            Text = "Meshes";

            if (RmdNodes.Length > 0 && RmdNodes.First().ElementType == 0x01)
                RmdNodes.First().Text = "Count Node";

            _SurrogateObject = new SurrogateDataObject(this);
        }

        public int GetMeshCount()
        {
            DataElement data = _ContainerElement.Elements[0] as DataElement;

            return *(ushort*)data.Data;
        } 

        public new class SurrogateDataObject : ContainerNode.SurrogateDataObject
        {
            protected MeshSetNode _MeshSetNode = null;

            [Category("Meshes")]
            public int MeshCount { get { return _MeshSetNode.MeshCount; } }

            public SurrogateDataObject(MeshSetNode owner)
                : base(owner)
            {
                _MeshSetNode = owner;
            }
        }
    }
}
