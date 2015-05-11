using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RMDEditor.Node
{
    public unsafe class RawFileDataNode : FileDataNode, IDisposable
    {
        public byte* Data { get { return _Data; } }
        private byte* _Data = null;

        public int DataSize { get { return _DataSize; } }
        private int _DataSize = 0;

        public byte[] BufferedData { get { return GetBufferedData(); } }

        public RawFileDataNode(string name, int size)
            : base(name)
        {
            _Data = (byte*)Marshal.AllocHGlobal(size);
            _DataSize = size;

            _SurrogateObject = new SurrogateDataObject(this);
            ContextMenuStrip.Items["Import"].Visible = false;
        }

        public RawFileDataNode(string name, byte[] data)
            : base(name)
        {
            _Data = (byte*)Marshal.AllocHGlobal(data.Length);
            _DataSize = data.Length;

            for (int i = 0; i < data.Length; i++)
                _Data[i] = data[i];

            _SurrogateObject = new SurrogateDataObject(this);
            ContextMenuStrip.Items["Import"].Visible = false;
        }

        public byte[] GetBufferedData()
        {
            byte[] result = new byte[_DataSize];

            for (int i = 0; i < result.Length; i++)
                result[i] = _Data[i];

            return result;
        }


        public override int GetSizeInMemory()
        {
            return _DataSize;
        }

        public override int GetHeaderSize()
        {
            return 0;
        }

        protected override FileDataNode FromFile(string path)
        {
            return NodeFactory.FromFile(path, true);
        }

        protected override byte[] ToFile()
        {
            return GetBufferedData();
        }

        #region IDisposable Members

        public bool Disposed { get { return _Disposed; } }
        protected bool _Disposed = false;

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_Disposed)
                return;

            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            Marshal.FreeHGlobal((IntPtr)_Data);

            _Disposed = true;
        }

        #endregion

        public new class SurrogateDataObject : FileDataNode.SurrogateDataObject, IDataSurrogate
        {
            protected RawFileDataNode _RawFileDataNode = null;

            [Category("Data")]
            [Browsable(false)]
            public byte* Data { get { return _RawFileDataNode.Data; } }

            [TypeConverter(typeof(NodeDataConverter))]
            [Category("Data")]
            public int DataSize { get { return _RawFileDataNode.DataSize; } }

            [Category("Data")]
            [Browsable(false)]
            public byte[] BufferedData { get { return _RawFileDataNode.BufferedData; } }

            public SurrogateDataObject(RawFileDataNode owner)
                : base(owner)
            {
                _RawFileDataNode = owner;
            }
        }
    }
}
