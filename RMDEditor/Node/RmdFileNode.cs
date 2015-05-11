using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using RMDEditor.Rmd;

namespace RMDEditor.Node
{
    public class RmdFileNode : FileDataNode, IContainerNode
    {
        public string FileName { get { return _Element.Name; } set { _Element.Name = value; Text = value; } }

        public RmdFileElement Element { get { return _Element; } }
        protected RmdFileElement _Element = null;


        public IContainerElement ContainerElement { get { return _Element; } }
        public RmdNode[] RMDNodes { get { return Nodes.OfType<RmdNode>().ToArray(); } }

        public IContainerNode RmdRoot { get { return this; } }
        public IContainerNode RmdParent { get { return null; } }

        public RmdFileNode(RmdFileElement elem)
            : base(elem.Name)
        {
            _Element = elem;
            _SurrogateObject = new SurrogateDataObject(this);

            foreach (Element child in elem.Elements)
                Nodes.Add(NodeFactory.FromElement(child));
        }

        public override int GetSizeInMemory()
        {
            return _Element.SizeInMemory;
        }

        public override int GetHeaderSize()
        {
            return 0;
        }

        public override void Insert(int index, FileDataNode node)
        {
            RmdNode rmdNode = node as RmdNode;

            if (rmdNode != null)
                _Element.Elements.Insert(index, rmdNode.Element);
            else
                throw new Exception("Rmd file nodes can only contain other Rmd nodes.");

            base.Insert(index, node);
        }

        public override void Remove(FileDataNode node)
        {
            RmdNode rmdNode = node as RmdNode;

            if (rmdNode != null)
                _Element.Elements.Remove(rmdNode.Element);

            base.Remove(node);
        }

        protected override FileDataNode FromFile(string path)
        {
            return NodeFactory.FromFile(path, true);
        }

        protected override byte[] ToFile()
        {
            return _Element.ToFile();
        }


        public new class SurrogateDataObject : FileDataNode.SurrogateDataObject
        {
            private RmdFileNode _RmdFileNode = null;

            [Category("File")]
            public string Name { get { return _RmdFileNode.FileName; } set { _RmdFileNode.FileName = value; } }

            public SurrogateDataObject(RmdFileNode owner)
                : base(owner)
            {
                _RmdFileNode = owner;
            }
        }
    }
}
