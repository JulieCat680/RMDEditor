using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using RMDEditor.FLW0;

namespace RMDEditor.Node
{
    public class Flw0Node : FileDataNode
    {
        public Flw0 Data { get { return _Data; } }
        private Flw0 _Data = null;

        public Flw0Node(string fileName, Flw0 fileData)
            : base(fileName)    
        {
            _Data = fileData;

            TreeNode sceneTracksNode = new TreeNode("Scene Tracks");
            Msg1Node msg1Node = new Msg1Node(_Data.Msg1);

            foreach (FlowTrack track in _Data.Tracks)
                sceneTracksNode.Nodes.Add(new FlowTrackNode(track));

            Nodes.Add(sceneTracksNode);
            Nodes.Add(msg1Node);
        }

        public override int GetHeaderSize()
        {
            throw new NotImplementedException();
        }

        public override int GetSizeInMemory()
        {
            throw new NotImplementedException();
        }

        protected override FileDataNode FromFile(string path)
        {
            throw new NotImplementedException();
        }

        protected override byte[] ToFile()
        {
            throw new NotImplementedException();
        }
    }
}
