using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RMDEditor.Pac;

namespace RMDEditor.Node
{
    public unsafe class PacDataNode : FileDataNode
    {
        public PacData PacData { get { return _PacData; } }
        private PacData _PacData = null;

        public byte* Data { get { return _PacData.Data; } }
        public int DataSize { get { return _PacData.DataSize; } }

        public byte[] BufferedData { get { return _PacData.BufferedData; } }

        public PacDataNode(PacData data)
            : base(data.Name)
        {
            _PacData = data;

            _SurrogateObject = new SurrogateDataObject(this);
            ContextMenuStrip.Items["Import"].Visible = false;
        }

        public override int GetHeaderSize()
        {
            return -1;
        }

        public override int GetSizeInMemory()
        {
            return _PacData.DataSize;
        }

        protected override FileDataNode FromFile(string path)
        {
            return NodeFactory.FromFile(path, true);
        }

        protected override byte[] ToFile()
        {
            return BufferedData;
        }

        public new class SurrogateDataObject : FileDataNode.SurrogateDataObject, IDataSurrogate
        {
            protected PacDataNode _PacDataNode = null;

            [Category("Data")]
            [Browsable(false)]
            public byte* Data { get { return _PacDataNode.Data; } }

            [TypeConverter(typeof(NodeDataConverter))]
            [Category("Data")]
            public int DataSize { get { return _PacDataNode.DataSize; } }

            [Category("Data")]
            [Browsable(false)]
            public byte[] BufferedData { get { return _PacDataNode.BufferedData; } }

            public SurrogateDataObject(PacDataNode owner)
                : base(owner)
            {
                _PacDataNode = owner;
            }
        }
    }
}
