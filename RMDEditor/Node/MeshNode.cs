using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RMDEditor.Rmd;

namespace RMDEditor.Node
{
    public class MeshNode : ContainerNode
    {
        public MeshNode(ContainerElement elem)
            : base(elem)
        {
            Text = "Mesh";

            Nodes[0].Text = "Mesh Data";
            Nodes[2].Text = "Ex Data";
        }
    }
}
