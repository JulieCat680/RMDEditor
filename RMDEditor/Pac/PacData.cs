using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RMDEditor.Pac
{
    public unsafe class PacData : IDisposable
    {
        public string Name { get { return GetName(); } set { SetName(value); } }
        private string _Name = null;

        public byte* Data { get { return _Data; } }
        private byte* _Data = null;
        public int DataSize { get { return _DataSize; } }
        private int _DataSize = 0;

        public int SizeInMemory { get { return GetSizeInMemory(); } }

        public byte[] BufferedData { get { return GetBufferedData(); } }

        public PacData(string name, int size)
        {
            SetName(name);

            _DataSize = size;
            _Data = (byte*)Marshal.AllocHGlobal(_DataSize);
        }

        public PacData(string name, byte[] data)
        {
            SetName(name);

            _DataSize = data.Length;
            _Data = (byte*)Marshal.AllocHGlobal(_DataSize);
            for (int i = 0; i < data.Length; i++)
                _Data[i] = data[i];
        }

        public PacData(string name, byte* data, int size)
        {
            SetName(name);

            _DataSize = size;
            _Data = (byte*)Marshal.AllocHGlobal(_DataSize);
            for (int i = 0; i < size; i++)
                _Data[i] = data[i];
        }

        ~PacData()
        {
            Dispose(false);
        }

        public int GetSizeInMemory()
        {
            return 0x100 + (_DataSize + 0x3F) & ~0x3F;
        }

        public byte[] GetBufferedData()
        {
            byte[] result = new byte[_DataSize];

            for (int i = 0; i < result.Length; i++)
                result[i] = _Data[i];

            return result;
        }

        public string GetName()
        {
            return _Name;
        }

        public void SetName(string value)
        {
            if (value == _Name)
                return;

            if (Encoding.ASCII.GetByteCount(value) >= 0xFC)
                throw new Exception("Pac data element names are limited to 252 characters.");

            _Name = value;
        }

        public void ExportData(string path)
        {
            File.WriteAllBytes(path, GetBufferedData());
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

        #region Static Members

        public static PacData[] FromFile(string path)
        {
            byte[] data = File.ReadAllBytes(path);

            return FromPtr(data, data.Length);
        }

        public static PacData FromPtr(byte[] data)
        {
            fixed (byte* ptr = data)
                return FromPtr(ptr);
        }

        public static PacData[] FromPtr(byte[] data, int size)
        {
            fixed (byte* ptr = data)
                return FromPtr(ptr, size);
        }

        public static PacData FromPtr(byte* ptr)
        {
            string name = new string((sbyte*)ptr);
            int nameBytecount = Encoding.ASCII.GetByteCount(name);
            int size = *(int*)(ptr + 0xFC);
            
            if (name.Any(x => x != 0x00 && char.IsControl(x)))
                throw new Exception("Data could not be opened as PAC data.");

            if (nameBytecount >= 0xFC)
                throw new Exception("Data could not be opened as PAC data.");

            for (int i = nameBytecount; i < 0xFC; i++)
                if (ptr[i] != 0x00)
                    throw new Exception("Data could not be opened as PAC data.");

            if (string.IsNullOrEmpty(name) && size == 0)
                return null;

            return new PacData(name, ptr + 0x100, size);
        }

        public static PacData[] FromPtr(byte* ptr, int size)
        {
            List<PacData> result = new List<PacData>();
            int offset = 0;

            while (offset < size)
            {
                byte* data = ptr + offset;
                string name = new string((sbyte*)data);
                int nameBytecount = Encoding.ASCII.GetByteCount(name);
                int sz = *(int*)(data + 0xFC);

                if (name.Any(x => x != 0x00 && char.IsControl(x)))
                    throw new Exception("Data could not be opened as PAC data.");

                if (nameBytecount >= 0xFC)
                    throw new Exception("Data could not be opened as PAC data.");

                for (int i = nameBytecount; i < 0xFC; i++)
                    if (data[i] != 0x00)
                        throw new Exception("Data could not be opened as PAC data.");

                if (sz + offset >= size)
                    throw new Exception("Data could not be opened as PAC data.");

                if (!string.IsNullOrEmpty(name) && sz > 0)
                {
                    PacData pacData = new PacData(name, data + 0x100, sz);
                    result.Add(pacData);
                    offset += (pacData.SizeInMemory + 0x3F) & ~0x3F;
                }
                else
                {
                    offset += 0x100;
                }
            }

            return result.ToArray();
        }

        public static byte[] ToFile(PacData[] pacData)
        {
            int size = 0;

            foreach (PacData item in pacData)
                size += item.SizeInMemory;

            size += 0x100;

            byte[] result = new byte[size];
            fixed (byte* ptr = result)
            {
                byte* data = ptr;

                foreach (PacData item in pacData)
                {
                    byte[] name = Encoding.ASCII.GetBytes(item.Name + '\0');
                    byte* nameData = data;
                    int* sizeData = (int*)(data + 0xFC);
                    byte* innerData = data + 0x100;

                    for (int i = 0; i < 0xFC; i++)
                        if (i < name.Length)
                            nameData[i] = name[i];
                        else
                            nameData[i] = 0;

                    sizeData[0] = item.DataSize;

                    int roundedSize = (item.DataSize + 0x3F) & ~0x3F;
                    for (int i = 0; i < roundedSize; i++)
                        if (i < item.DataSize)
                            innerData[i] = item.Data[i];
                        else
                            innerData[i] = 0;

                    data += item.SizeInMemory;
                }

                for (int i = 0; i < 0x100; i++)
                    data[i] = 0;
            }

            return result;
        }

        public static void ToFile(string path, PacData[] data)
        {
            File.WriteAllBytes(path, ToFile(data));
        }

        #endregion
    }
}
