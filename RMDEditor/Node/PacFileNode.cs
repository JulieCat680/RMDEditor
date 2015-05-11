using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using RMDEditor.Rmd;
using RMDEditor.Pac;

namespace RMDEditor.Node
{
    public unsafe class PacFileNode : FileDataNode
    {
        public string FileName { get { return _FileName; } set { _FileName = value; Text = value; } }
        private string _FileName = null;

        public SurrogateDataObject DataSurrogate { get { return _SurrogateObject as SurrogateDataObject; } }

        public PacFileNode(string fileName, ICollection<PacData> data)
            : base(fileName)
        {
            _FileName = fileName;

            foreach (PacData item in data)
            {
                if (item.Name.EndsWith(".Rmd", true, CultureInfo.CurrentCulture))
                    Nodes.Add(NodeFactory.FromElement(RmdFileElement.FromPtr(item.Name, item.Data, item.DataSize)));
                else
                    Nodes.Add(new PacDataNode(item));
            }

            _SurrogateObject = new SurrogateDataObject(this);
        }

        public override int GetSizeInMemory()
        {
            int size = 0;

            foreach (FileDataNode item in Nodes.OfType<FileDataNode>())
            {
                size += 0x100;
                size += (item.SizeInMemory + 0x3F) & ~0x3F;
            }

            size += 0x100;

            return size;
        }

        public override int GetHeaderSize()
        {
            return 0;
        }

        public override int OffsetOfNode(FileDataNode node)
        {
            if (node == this)
                return 0;

            int offset = GetHeaderSize();
            foreach (FileDataNode child in Nodes.OfType<FileDataNode>())
            {
                offset += 0x100;

                if (child == node)
                    return offset;

                int childOffset = child.OffsetOfNode(node);
                if (childOffset != -1)
                    return offset + childOffset;

                offset += (child.GetSizeInMemory() + 0x3F) & ~0x3F;
            }

            return -1;
        }

        protected override FileDataNode FromFile(string path)
        {
            return NodeFactory.FromFile(path, false);
        }

        protected override byte[] ToFile()
        {
            PacData[] pacData = Nodes.OfType<FileDataNode>().Select(x => new PacData(x.Text, x.Export())).ToArray();
            return PacData.ToFile(pacData);
        }

        public new class SurrogateDataObject : FileDataNode.SurrogateDataObject
        {
            protected PacFileNode _PacFileNode = null;

            [Category("File")]
            public string Name { get { return _PacFileNode.FileName; } set { _PacFileNode.FileName = value; } }

            public SurrogateDataObject(PacFileNode owner)
                : base(owner)
            {
                _PacFileNode = owner;
            }
        }
    }
}
