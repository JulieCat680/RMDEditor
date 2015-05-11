using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RMDEditor.Rmd;

namespace RMDEditor.Node
{
    public class ContainerNode : RmdNode, IContainerNode
    {
        public IContainerElement ContainerElement { get { return _ContainerElement; } }
        protected ContainerElement _ContainerElement = null;

        public RmdNode[] RmdNodes { get { return Nodes.OfType<RmdNode>().ToArray(); } }

        public ContainerNode(ContainerElement elem)
            : base(elem)
        {
            _ContainerElement = elem;

            foreach (Element child in elem.Elements)
                Nodes.Add(NodeFactory.FromElement(child));
        }

        public override void Insert(int index, FileDataNode node)
        {
            RmdNode rmdNode = node as RmdNode;

            if (rmdNode == null)
                throw new Exception("Rmd container nodes can only contain other Rmd nodes.");

            _ContainerElement.Elements.Insert(index, rmdNode.Element);
            base.Insert(index, node);
        }

        public override void Remove(FileDataNode node)
        {
            RmdNode rmdNode = node as RmdNode;

            if (rmdNode != null)
                _ContainerElement.Elements.Remove(rmdNode.Element);

            base.Remove(node);
        }
    }
}
