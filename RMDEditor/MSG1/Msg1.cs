using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RMDEditor.MSG1
{
    public unsafe class Msg1
    {
        public int UnknownFlag { get { return _UnknownFlag; } set { _UnknownFlag = value; } }
        private int _UnknownFlag = 0;

        public List<MessageData> Messages { get { return _Messages; } }
        private List<MessageData> _Messages = null;

        public byte[] UnknownData { get { return _UnknownData; } set { _UnknownData = value; } }
        private byte[] _UnknownData = null;

        public Msg1()
        {
            _Messages = new List<MessageData>();
            _UnknownData = new byte[0];
        }

        public Msg1(IEnumerable<MessageData> messages, byte[] unknownData)
        {
            _Messages = new List<MessageData>();
            _UnknownData = unknownData;

            _Messages.AddRange(messages);
        }

        public int GetHeaderSize()
        {
            return Header.SIZE;
        }

        public int GetSizeInMemory()
        {
            /*
            int headerSize = GetHeaderSize();
            int messageEntrySize = _Messages.Count * MessageData.MessageEntry.SIZE;
            int messagesSize = _Messages.Count * MessageData.Message.SIZE;
            int messageTextSize = 0;
            int speakerTextSize = 0;

            List<string> messageTexts = new List<string>();
            List<string> speakerTexts = new List<string>();
            foreach (MessageData message in _Messages)
            {
                if (!messageTexts.Contains(message.Text))
                    messageTexts.Add(message.Text);

                if (!speakerTexts.Contains(message.SpeakerName))
                    speakerTexts.Add(message.SpeakerName);
            }
            
            messageTextSize = messageTexts.Sum(x => Encoding.ASCII.GetByteCount(x));
            speakerTextSize = speakerTexts.Sum(x => Encoding.ASCII.GetByteCount(x));

            int sizeInMemory = 0;
            sizeInMemory += headerSize;
            sizeInMemory += messageEntrySize;
            sizeInMemory += messagesSize;
            sizeInMemory += messageTextSize;
            sizeInMemory += speakerTextSize;

            return sizeInMemory;
            */

            return 0;
        }

        #region Static Members

        public static Msg1 FromFile(string path)
        {
            byte[] data = File.ReadAllBytes(path);

            return FromPtr(data);
        }

        public static Msg1 FromPtr(byte[] data)
        {
            fixed (byte* ptr = data)
                return FromPtr(ptr);
        }

        public static Msg1 FromPtr(byte* ptr)
        {
            Header* header = (Header*)ptr;

            if (header->tag != 0x3147534D) // MSG1
                throw new FormatException("Data could not be wrapped with Msg1 class.");

            Msg1 result = new Msg1();
            result._UnknownFlag = header->unk0;
            result._Messages.AddRange(MessageData.FromPtr(ptr + Header.SIZE, header->messageCount));
            result._UnknownData = new byte[header->unkDataLength];

            byte* unknownData = ptr + header->unkDataOffset;
            for (int i = 0; i < result._UnknownData.Length; i++)
                result._UnknownData[i] = unknownData[i];

            return result;
        }

        #endregion

        #region Wrapper Classes

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct Header
        {
            public const int SIZE = 0x20;

            public int      unk0;
            public int      size;
            public int      tag; // MSG1
            public int      pad0;
            public int      unkDataOffset;
            public int      unkDataLength;
            public int      messageCount;
            public int      unk1;
        }

        #endregion
    }
}
