using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RMDEditor.Rmd;

namespace RMDEditor.Node
{
    public unsafe class MaterialSetNode : ContainerNode
    {
        public int MaterialCount { get { return GetMaterialCount(); } }

        public MaterialSetNode(ContainerElement elem)
            : base(elem)
        {
            Text = "Materials";

            if (RmdNodes.Length > 0 && RmdNodes.First().ElementType == 0x01)
                RmdNodes.First().Text = "Material Data";

            _SurrogateObject = new SurrogateDataObject(this);
        }

        public int GetMaterialCount()
        {
            DataElement data = _ContainerElement.Elements[0] as DataElement;

            return *(ushort*)data.Data;
        } 

        public new class SurrogateDataObject : ContainerNode.SurrogateDataObject
        {
            protected MaterialSetNode _MaterialSetNode = null;

            [Category("Materials")]
            public int MaterialCount { get { return _MaterialSetNode.MaterialCount; } }

            public SurrogateDataObject(MaterialSetNode owner)
                : base(owner)
            {
                _MaterialSetNode = owner;
            }
        }
    }
}
