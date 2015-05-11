using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMDEditor.Rmd
{
    public unsafe class RmdFileElement : IContainerElement
    {
        public string Name { get { return _Name; } set { _Name = value; } }
        private string _Name = null;

        public List<Element> Elements { get { return _Elements; } }
        protected List<Element> _Elements = null;

        public int ElementCount { get { return _Elements.Count; } }
        public int TotalElementCount { get { return GetTotalElementCount(); } }

        public int SizeInMemory { get { return GetSizeInMemory(); } }



        public RmdFileElement(string name)
        {
            _Name = name;
            _Elements = new List<Element>();
        }

        public RmdFileElement(string name, IEnumerable<Element> elements)
        {
            _Name = name;
            _Elements = new List<Element>();
            _Elements.AddRange(elements);
        }



        public int GetSizeInMemory()
        {
            int size = 0;

            foreach (Element elem in _Elements)
                size += elem.GetExSizeInMemory(size);

            return size;
        }

        public int GetTotalElementCount()
        {
            int count = _Elements.Count;

            foreach (ContainerElement elem in _Elements.OfType<ContainerElement>())
                count += elem.GetTotalElementCount();

            return count;
        }

        public int OffsetOfElement(Element elem)
        {
            int offset = 0;

            for (int i = 0; i < _Elements.Count; i++)
            {
                if (_Elements[i] == elem)
                    return offset;
                else if (_Elements[i] is ContainerElement)
                {
                    int subOffset = (_Elements[i] as ContainerElement).OffsetOfElement(elem);

                    if (subOffset != -1)
                        return offset + subOffset;
                }

                offset += elem.GetExSizeInMemory(offset);
            }

            return -1;
        }

        public Element ToRmdElement()
        {
            if (_Elements.Count == 1)
                return _Elements[0];

            return new ContainerElement(0, 0, _Elements);
        }

        public void ToFile(string path)
        {
            File.WriteAllBytes(path, ToFile());
        }

        public byte[] ToFile()
        {
            byte[] result = new byte[GetSizeInMemory()];

            fixed (byte* ptr = result)
            {
                int offset = 0;
                byte* data = ptr + offset;
                foreach (Element child in _Elements)
                {
                    byte[] childData = child.ToFileEx(offset); ;

                    for (int i = 0; i < childData.Length; i++)
                        data[i] = childData[i];

                    offset += childData.Length;
                    data = ptr + offset;
                }
            }

            return result;
        }

        #region Static Members

        public static RmdFileElement FromFile(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            return FromPtr(Path.GetFileName(path), data);
        }

        public static RmdFileElement FromPtr(string name, byte[] data)
        {
            fixed (byte* ptr = data)
                return FromPtr(name, ptr, data.Length);
        }

        public static RmdFileElement FromPtr(string name, byte* ptr, int size)
        {
            Element[] elements = Element.FromPtr(ptr, size);

            return new RmdFileElement(name, elements);
        }

        #endregion
    }
}
