using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RMDEditor.Rmd
{
    [DebuggerDisplay("Type={Type}, ElementCount={ElementCount}")]
    public unsafe class ContainerElement : Element, IContainerElement
    {
        public List<Element> Elements { get { return _Elements; } }
        protected List<Element> _Elements = null;

        public int ElementCount { get { return _Elements.Count; } }
        public int TotalElementCount { get { return GetTotalElementCount(); } }

        public ContainerElement(uint type, uint tag)
            : base(type, tag)
        {
            _Elements = new List<Element>();
        }

        public ContainerElement(uint type, uint tag, IEnumerable<Element> elements)
            : base(type, tag)
        {
            _Elements = new List<Element>();
            _Elements.AddRange(elements);
        }

        public override int GetSizeInMemory()
        {
            int size = ElementWrapper.SIZE;

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
            int offset = ElementWrapper.SIZE;

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

        public override byte[] ToFile()
        {
            if (IsExElement())
                return ToFileEx(0);

            byte[] result = new byte[GetSizeInMemory()];
            int dataSize = GetSizeInMemory() - ElementWrapper.SIZE;

            fixed (byte* ptr = result)
            {
                ElementWrapper* wrapper = (ElementWrapper*)ptr;
                wrapper->type = _Type;
                wrapper->size = dataSize;
                wrapper->tag = _Tag;

                int offset = 0;
                byte* data = ptr + ElementWrapper.SIZE + offset;
                foreach (Element child in _Elements)
                {
                    byte[] childData = child.ToFileEx(offset);

                    for (int i = 0; i < childData.Length; i++)
                        data[i] = childData[i];

                    offset += childData.Length;
                    data = ptr + ElementWrapper.SIZE + offset;
                }
            }

            return result;
        }

        public override byte[] ToFileEx(int offset)
        {
            if (!IsExElement())
                return ToFile();

            byte[] result = new byte[GetSizeInMemory()];
            int dataSize = GetSizeInMemory() - ElementWrapper.SIZE;

            fixed (byte* ptr = result)
            {
                ElementWrapper* wrapper = (ElementWrapper*)ptr;
                wrapper->type = _Type;
                wrapper->size = dataSize;
                wrapper->tag = _Tag;

                int offs = 0;
                byte* data = ptr + ElementWrapper.SIZE + offs;
                foreach (Element child in _Elements)
                {
                    byte[] childData = child.ToFileEx(offset + offs);

                    for (int i = 0; i < childData.Length; i++)
                        data[i] = childData[i];

                    offs += childData.Length;
                    data = ptr + ElementWrapper.SIZE + offs;
                }
            }

            return result;
        }

        #region Static Members

        public static ContainerElement FromPtr(byte[] data)
        {
            fixed (byte* ptr = data)
                return FromPtr(ptr);
        }

        public static new ContainerElement FromPtr(byte* ptr)
        {
            ElementWrapper* wrapper = (ElementWrapper*)ptr;
            Element[] elements = Element.FromPtr(ptr + ElementWrapper.SIZE, wrapper->size);

            return new ContainerElement(wrapper->type, wrapper->tag, elements);
        }

        #endregion
    }
}
