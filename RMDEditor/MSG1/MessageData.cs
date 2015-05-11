using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices; 
using System.Text;
using System.Threading.Tasks;

namespace RMDEditor.MSG1
{
    [DebuggerDisplay("Name={Name}, Text={Text}")]
    public unsafe class MessageData
    {
        public string Name { get { return _Name; } set { _Name = value; } }
        private string _Name = null;

        public string SpeakerName { get { return _SpeakerName; } set { _SpeakerName = value; } }
        private string _SpeakerName = null;

        public List<string> Text { get { return _Text; } set { _Text = value; } }
        private List<string> _Text = null;

        public MessageData() : this(null, null) { }

        public MessageData(string name, string speakerName)
        {
            _Name = name;
            _SpeakerName = speakerName;
            _Text = new List<string>();
        }

        #region Static Members

        public static MessageData[] FromPtr(byte[] data, int count)
        {
            fixed (byte* ptr = data)
                return FromPtr(ptr, count);
        }

        public static MessageData[] FromPtr(byte* ptr, int count)
        {
            MessageEntry * entries = (MessageEntry*)ptr;
            int* speakers = (int*)(ptr + *(int*)(ptr + count * MessageEntry.SIZE));
            int speakerCount = *(int*)(ptr + count * MessageEntry.SIZE + 0x4);

            string[] speakerNames = new string[speakerCount];
            for (int i = 0; i < speakerNames.Length; i++)
                speakerNames[i] = new string((sbyte*)(ptr + speakers[i]));

            MessageData[] result = new MessageData[count];
            for (int i = 0; i < result.Length; i++)
            {
                if (entries[i].type == 0)
                {
                    Message* message = (Message*)(ptr + entries[i].offset);
                    result[i] = new MessageData();
                    result[i]._Name = new string((sbyte*)message->messageName);

                    if (message->speakerId != -1)
                        result[i]._SpeakerName = speakerNames[message->speakerId];

                    for (int j = 0; j < message->numStrings; j++)
                    {
                        sbyte* stringData = (sbyte*)(ptr + message->offsets[j]);
                        result[i]._Text.Add(Encoding.ASCII.GetString(stringData));
                    }
                }
                else if (entries[i].type == 1)
                {
                    

                }
                else
                {
                    Debugger.Break();
                }
            }

            return result;
        }

        #endregion

        #region Wrapper Classes

        [StructLayout(LayoutKind.Sequential, Pack=0)]
        public struct MessageEntry
        {
            public const int SIZE = 0x8;

            public int         type;
            public int         offset;
        }

        [StructLayout(LayoutKind.Sequential, Pack=0)]
        public struct Message
        {
            public fixed byte  messageName[0x18];
            public short       numStrings;
            public short       speakerId;
            public fixed int   offsets[1]; // offsets[numStrings]

            //public int         dataLength;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct MessageChoice
        {
            public fixed byte   choiceName[0x18];
            public short        unk0;
            public short        numStrings;
            public fixed int    offsets[1]; // offsets[numStrings]

            // public int       dataLength;
        }

        #endregion
    }
}
