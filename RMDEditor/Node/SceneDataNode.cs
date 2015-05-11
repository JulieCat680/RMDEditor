using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RMDEditor.Rmd;

namespace RMDEditor.Node
{
    public unsafe class SceneDataNode : ContainerNode
    {
        public int DrawCount { get { return GetDrawCount(); } }

        public SceneDataNode(ContainerElement elem)
            : base(elem)
        {
            Text = "Scene Data";

            if (RmdNodes.Length > 0 && RmdNodes.First().ElementType == 0x01)
                RmdNodes.First().Text = "Count Node";

            if (RmdNodes.Length > 1 && RmdNodes.Last().ElementType == 0x03)
                RmdNodes.Last().Text = "Ex Data";

            _SurrogateObject = new SurrogateDataObject(this);
        }

        public int GetDrawCount()
        {
            DataElement data = _ContainerElement.Elements[0] as DataElement;

            return *(ushort*)data.Data;
        } 

        public new class SurrogateDataObject : ContainerNode.SurrogateDataObject
        {
            protected SceneDataNode _SceneDataNode = null;

            [Category("Scene")]
            public int DrawCount { get { return _SceneDataNode.DrawCount; } }

            public SurrogateDataObject(SceneDataNode owner)
                : base(owner)
            {
                _SceneDataNode = owner;
            }
        }
    }
}
