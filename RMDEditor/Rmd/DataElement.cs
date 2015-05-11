using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RMDEditor.Rmd
{
    [DebuggerDisplay("Type={Type}, DataSize={DataSize}")]
    public unsafe class DataElement : Element, IDisposable
    {
        public byte* Data { get { return _Data; } }
        protected byte* _Data = null;
        public int DataSize { get { return _DataSize; } }
        protected int _DataSize = 0;

        public byte[] BufferedData { get { return GetBufferedData(); } }

        public byte[] ExData { get { return _ExData; } }
        private byte[] _ExData = null;

        public DataElement(uint type, uint tag, int size)
            : base(type, tag)
        {
            _Data = (byte*)Marshal.AllocHGlobal(_DataSize);
            _DataSize = size;
        }

        public DataElement(uint type, uint tag, byte[] data)
            : base(type, tag)
        {
            _Data = (byte*)Marshal.AllocHGlobal(data.Length);
            _DataSize = data.Length;

            for (int i = 0; i < _DataSize; i++)
                _Data[i] = data[i];
        }

        public DataElement(uint type, uint tag, byte[] data, byte[] exData)
            : this(type, tag, data)
        {
            _ExData = new byte[exData.Length];
            for (int i = 0; i < _ExData.Length; i++)
                _ExData[i] = exData[i];
        }

        ~DataElement()
        {
            Dispose(false);
        }

        public void Resize(int size)
        {
            _Data = (byte*)Marshal.ReAllocHGlobal((IntPtr)_Data, (IntPtr)size);
            _DataSize = size;
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
            if (IsExElement())
                return ExtensionWrapper.FULLSIZE + _DataSize;
            else
                return ElementWrapper.SIZE + _DataSize;
        }

        public override byte[] ToFile()
        {
            if (IsExElement())
                return ToFileEx(0);

            byte[] result = new byte[GetSizeInMemory()];

            fixed (byte* ptr = result)
            {
                ElementWrapper* wrapper = (ElementWrapper*)ptr;
                wrapper->type = _Type;
                wrapper->size = _DataSize;
                wrapper->tag = _Tag;

                byte* data = ptr + ElementWrapper.SIZE;
                for (int i = 0; i < _DataSize; i++)
                    data[i] = _Data[i];
            }

            return result;
        }

        public override byte[] ToFileEx(int offset)
        {
            if (!IsExElement())
                return ToFile();

            byte[] result = new byte[GetExSizeInMemory(offset)];

            fixed (byte* ptr = result)
            {
                ElementWrapper* wrapper = (ElementWrapper*)ptr;
                wrapper->type = _Type;
                wrapper->size = _DataSize + ExtensionWrapper.EXSIZE;
                wrapper->tag = _Tag;

                byte* exData = ptr + ElementWrapper.SIZE;
                for (int i = 0; i < ExtensionWrapper.EXSIZE; i++)
                    if (_ExData != null && i < _ExData.Length)
                        exData[i] = _ExData[i];
                    else
                        exData[i] = 0;

                int z = ((0x10 - (offset & 0xF)) & 0xF);
                byte* data = ptr + ExtensionWrapper.FULLSIZE + ((0x10 - (offset & 0xF)) & 0xF);
                for (int i = 0; i < _DataSize; i++)
                    data[i] = _Data[i];
            }

            return result;
        }

        #region Static Members

        public static DataElement FromPtr(byte[] data)
        {
            fixed (byte* ptr = data)
                return FromPtr(ptr);
        }

        public new static DataElement FromPtr(byte* ptr)
        {
            ElementWrapper* wrapper = (ElementWrapper*)ptr;
            int size = wrapper->size;

            byte* dataPtr = ptr + ElementWrapper.SIZE;
            byte[] data = new byte[size];
            for (int i = 0; i < data.Length; i++)
                data[i] = dataPtr[i];

            return new DataElement(wrapper->type, wrapper->tag, data);
        }

        public new static DataElement FromExPtr(byte* ptr, byte* dataPtr)
        {
            ExtensionWrapper* wrapper = (ExtensionWrapper*)ptr;
            ElementWrapper* elem = (ElementWrapper*)ptr;
            int size = wrapper->size;

            byte[] data = new byte[size];
            byte[] exData = new byte[ExtensionWrapper.EXSIZE];
            for (int i = 0; i < data.Length; i++)
                data[i] = dataPtr[i];

            for (int i = 0; i < exData.Length; i++)
                exData[i] = ptr[i + ElementWrapper.SIZE];

            return new DataElement(elem->type, elem->tag, data, exData);
        }

        #endregion

        #region IDisposable Members

        public bool Disposed { get { return _Disposed; } }
        protected bool _Disposed = false;

        public void Dispose()
        {
            Dispose(false);
        }

        public void Dispose(bool disposing)
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
    }
}
