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
    public unsafe abstract class Element
    {
        public uint Type { get { return _Type; } set { _Type = value; } }
        protected uint _Type = 0;

        public uint Tag { get { return _Tag; } set { _Tag = value; } }
        protected uint _Tag = 0;


        public Element(uint type, uint unk)
        {
            _Type = type;
            _Tag = unk;
        }

        public abstract int GetSizeInMemory();

        public int GetExSizeInMemory(int offset)
        {
            if (IsExElement())
                return GetSizeInMemory() + ((0x10 - (offset & 0xF)) & 0xF);
            else
                return GetSizeInMemory();
        }

        public void ToFile(string path)
        {
            File.WriteAllBytes(path, ToFile());
        }

        public abstract byte[] ToFile();

        public abstract byte[] ToFileEx(int offset);

        #region Static Members

        public static RmdFileElement FromFile(string path)
        {
            return RmdFileElement.FromFile(path);
        }

        public static Element FromPtr(byte* ptr)
        {
            ElementWrapper* wrapper = (ElementWrapper*)ptr;
            byte* data = ptr + ElementWrapper.SIZE;

            if (IsElement(data, wrapper->size))
                return ContainerElement.FromPtr(ptr);
            else
                return DataElement.FromPtr(ptr);
        }

        public static Element FromExPtr(byte* ptr, byte* data)
        {
            return DataElement.FromExPtr(ptr, data);
        }

        public static Element[] FromPtr(byte* ptr, int size)
        {
            List<Element> result = new List<Element>();
            int offset = 0;
            bool done = false;

            while (!done && offset < size)
            {
                Element elem = null;

                if (Element.IsElement(ptr + offset, size))
                {
                    if (IsExElement(ptr + offset))
                        elem = Element.FromExPtr(ptr + offset, ptr + ((offset + ExtensionWrapper.FULLSIZE + 0xF) & ~0xF));
                    else
                        elem = Element.FromPtr(ptr + offset);

                    result.Add(elem);
                    offset += elem.GetExSizeInMemory(offset);
                }
                else
                {
                    Element.ElementWrapper* wrapper = (Element.ElementWrapper*)(ptr + offset);
                    throw new Exception(string.Format("Unable to parse model element: type=0x{0:X8}, size=0x{1:X}, tag=0x{2:X8}", wrapper->type, wrapper->size, wrapper->tag));
                }
            }

            return result.ToArray();
        }

        public static bool IsElement(byte* ptr, int maxSize)
        {
            ElementWrapper* wrapper = (ElementWrapper*)ptr;

            return wrapper->size < maxSize
               && (wrapper->tag == ElementWrapper.TAG1 || wrapper->tag == ElementWrapper.TAG2);
        }

        public static bool IsExElement(byte* ptr)
        {
            ElementWrapper* wrapper = (ElementWrapper*)ptr;

            return wrapper->type == 0xF0F000E0;
        }

        public static bool IsExElement(uint type)
        {
            return type == 0xF0F000E0;
        }

        public bool IsExElement()
        {
            return IsExElement(_Type);
        }

        #endregion

        #region Wrapper Classes

        [StructLayout(LayoutKind.Sequential, Size = 0xC, Pack = 0)]
        public struct ElementWrapper
        {
            public const int    SIZE = 0xC;

            public const uint   TAG1 = 0x1C020037;
            public const uint   TAG2 = 0x40000000;


            public uint         type;
            public int          size;
            public uint         tag;
        }

        public struct ExtensionWrapper
        {
            public const int FULLSIZE = 0x20;
            public const int EXSIZE = 0x14;

            public ElementWrapper   element;
            public short            unk1;
            public short            unk2;
            public int              size;
            public uint             unk3;
            public uint             unk4;
            public uint             unk5;
        }

        #endregion
    }
}
