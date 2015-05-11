using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using RMDEditor.MSG1;

namespace RMDEditor.FLW0
{
    public unsafe class Flw0
    {
        public List<FlowTrack> Tracks { get { return _Tracks; } }
        private List<FlowTrack> _Tracks = null;

        

        public Msg1 Msg1 { get { return _Msg1; } }
        private Msg1 _Msg1 = null;

        public byte[] PadData { get { return _PadData; } set { _PadData = value; } }
        private byte[] _PadData = null;

        public Flw0()
        {
            _Tracks = new List<FlowTrack>();
            _Msg1 = new Msg1();
            _PadData = new byte[0xF0];
        }

        public int GetHeaderSize()
        {
            return Header.SIZE;
        }

        public int GetSizeInMemory()
        {
            /*
            int headerSize = GetHeaderSize();
            int primeTracksSize = Track.SIZE * _Tracks.Count;
            int secondTrackSize = Track.SIZE * _SecondaryTracks.Count;
            int trackDataSize = 0;
            int msg0Size = _Msg1.GetSizeInMemory();
            int padDataSize = _PadData.Length;

            foreach (FlowTrack track in Tracks)
                trackDataSize += sizeof(Int32) * track.Data.Count;

            foreach (FlowTrack track in SecondaryTracks)
                trackDataSize += sizeof(Int32) * track.Data.Count;

            int sizeInMemory = 0;
            sizeInMemory += headerSize;
            sizeInMemory += primeTracksSize;
            sizeInMemory += secondTrackSize;
            sizeInMemory += trackDataSize;
            sizeInMemory += msg0Size;
            sizeInMemory += padDataSize;

            return sizeInMemory;
             */

            return 0;
        }

        #region Static Members

        public static Flw0 FromFile(string path)
        {
            byte[] data = File.ReadAllBytes(path);

            return FromPtr(data);
        }

        public static Flw0 FromPtr(byte[] data)
        {
            fixed (byte* ptr = data)
                return FromPtr(ptr);
        }

        public static Flw0 FromPtr(byte* ptr)
        {
            Header* header = (Header*)ptr;

            if (header->tag != 0x30574C46) // FLW0
                throw new FormatException("Data could not be wrapped with Flw0 class.");

            Flw0 result = new Flw0();
            SectionEntry* sections = (SectionEntry*)(ptr + Header.SIZE);
            TrackLabel* trackLabels = (TrackLabel*)(ptr + sections[0].offset);
            TrackLabel* gotoLabels = (TrackLabel*)(ptr + sections[1].offset);
            int* trackData = (int*)(ptr + sections[2].offset);
            byte* msg1Data = (ptr + sections[3].offset);
            byte* padData = (ptr + sections[4].offset);

            for (int i = 0; i < sections[0].elementCount; i++)
            {
                FlowTrack track = new FlowTrack();
                track.Name = new string((sbyte*)trackLabels[i].name);

                int index = trackLabels[i].dataIndex;

                track.Data.Add(trackData[index]);
                while (trackData[index] != 0x9 && (index + 1) < sections[2].elementCount)
                    track.Data.Add(trackData[++index]);

                result._Tracks.Add(track);

                int startIndex = trackLabels[i].dataIndex;
                int endIndex = trackLabels[i].dataIndex + track.Data.Count;
                for (int j = 0; j < sections[1].elementCount; j++)
                    if (gotoLabels[j].dataIndex >= startIndex && gotoLabels[j].dataIndex < endIndex)
                    {
                        string name = new string((sbyte*)gotoLabels[j].name);
                        int trackIndex = gotoLabels[j].dataIndex - startIndex;
                        track.Labels.Add(new KeyValuePair<string, int>(name, trackIndex));
                    }
            }

            if (sections[3].elementCount > 0)
                result._Msg1 = Msg1.FromPtr(msg1Data);

            result._PadData = new byte[sections[4].elementCount];
            for (int i = 0; i < result._PadData.Length; i++)
                result._PadData[i] = padData[i];

            return result;
        }

        #endregion

        #region Wrapper Classes

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct Header
        {
            public const int    SIZE = 0x20;

            public int      unk0;
            public int      dataSize;
            public uint     tag; // FLW0
            public int      pad0;
            public int      sectionCount;
            public int      pad1;
            public int      pad2;
            public int      pad3;
        }

        [StructLayout(LayoutKind.Sequential, Pack=0)]
        public struct SectionEntry
        {
            public const int    SIZE = 0x10;

            public int      identifier;
            public int      elementSize;
            public int      elementCount;
            public int      offset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct TrackLabel
        {
            public const int SIZE = 0x20;

            public fixed byte   name[0x18];
            public int          dataIndex;
            public int          pad0;
        }

        #endregion
    }
}
