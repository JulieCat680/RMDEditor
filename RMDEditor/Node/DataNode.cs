using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RMDEditor.Rmd;


namespace RMDEditor.Node
{
    public unsafe class DataNode : RmdNode
    {
        public DataElement DataElement { get { return _DataElement; } }
        protected DataElement _DataElement = null;

        public byte* Data { get { return _DataElement.Data; } }
        public int DataSize { get { return _DataElement.DataSize; } }

        public byte[] BufferedData { get { return _DataElement.BufferedData; } }

        public DataNode(DataElement elem)
            : base(elem)
        {
            _DataElement = elem;
            _SurrogateObject = new SurrogateDataObject(this);

            ContextMenuStrip.Items["Import"].Visible = false;
        }

        public void Resize(int size)
        {
            _DataElement.Resize(size);
        }

        public override void Insert(int index, FileDataNode node)
        {
            throw new NotImplementedException();
        }

        public new class SurrogateDataObject : RmdNode.SurrogateDataObject, IDataSurrogate
        {
            protected DataNode _DataNode = null;

            [Category("Data")]
            [Browsable(false)]
            public byte* Data { get { return _DataNode.Data; } }

            [TypeConverter(typeof(NodeDataConverter))]
            [Category("Data")]
            public int DataSize { get { return _DataNode.DataSize; } }

            [Category("Data")]
            [Browsable(false)]
            public byte[] BufferedData { get { return _DataNode.BufferedData; } }

            public SurrogateDataObject(DataNode owner)
                : base(owner)
            {
                _DataNode = owner;
            }
        }
    }
}
