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
    public class RmdNode : FileDataNode
    {
        public Element Element { get { return _Element; } }
        protected Element _Element = null;

        public uint ElementType { get { return _Element.Type; } set { _Element.Type = value; } }
        public uint ElementTag { get { return _Element.Tag; } set { _Element.Tag = value; } }


        public IContainerNode RmdRoot { get { return (RmdParent != null ? RmdParent.RmdRoot : null); } }
        public IContainerNode RmdParent { get { return Parent as IContainerNode; } }

        public SurrogateDataObject DataSurrogate { get { return _SurrogateObject as SurrogateDataObject; } }

        public RmdNode(Element elem)
        {
            _Element = elem;
            _SurrogateObject = new SurrogateDataObject(this);

            Text = elem.Type.ToString("X8");
        }

        public override int GetSizeInMemory()
        {
            if (_Element.IsExElement())
                return _Element.GetExSizeInMemory(GetFileOffset());
            else
                return _Element.GetSizeInMemory();
        }

        public override int GetHeaderSize()
        {
            return Element.ElementWrapper.SIZE;
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
            [TypeConverter(typeof(NodeDataConverter))]
            [Category("Node")]
            public uint Type { get { return _RmdNode.ElementType; } set { _RmdNode.ElementType = value; } }

            [TypeConverter(typeof(NodeDataConverter))]
            [Category("Node")]
            public uint Tag { get { return _RmdNode.ElementTag; } set { _RmdNode.ElementTag = value; } }

            protected RmdNode _RmdNode = null;

            public SurrogateDataObject(RmdNode owner)
                : base(owner)
            {
                _RmdNode = owner;
            }
        }
    }
}
